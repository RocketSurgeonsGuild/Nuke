// Copyright 2019 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Rocket.Surgery.Nuke.AzurePipelines.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI;
using Nuke.Common;
using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Rocket.Surgery.Nuke.AzurePipelines
{
    [PublicAPI]
    public class RocketSurgeryAzurePipelinesAttribute : ConfigurationGenerationAttributeBase
    {
        public RocketSurgeryAzurePipelinesAttribute(string methodName) { MethodName = methodName; }

        public string MethodName { get; }
        public string[] InvokedTargets { get; set; } = new string[0];
        public string[] NonEntryTargets { get; set; } = new string[0];
        public string[] ExcludedTargets { get; set; } = new string[0];


        private string _configurationFile;
        public string ConfigurationFile
        {
            get
            {
                return _configurationFile ?? (_configurationFile = NukeBuild.RootDirectory / "azure-pipelines.yml");
            }
            set => _configurationFile = Path.IsPathRooted(value) ? value : NukeBuild.RootDirectory / value;
        }

        protected override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };

        protected override HostType HostType => HostType.AzurePipelines;

        protected override void Generate(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
        {
            var relevantTargets = InvokedTargets
                .SelectMany(x => ExecutionPlanner.GetExecutionPlan(executableTargets, new[] { x }))
                .Distinct()
                .Where(x => !ExcludedTargets.Contains(x.Name) && !NonEntryTargets.Contains(x.Name)).ToList();

            var method = build.GetType().GetMethod(MethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            var serializer = new SerializerBuilder()
                .WithTypeConverter(new TemplateReferenceConverter())
                .WithTypeConverter(new StageConverter())
                .WithTypeConverter(new JobConverter())
                .WithTypeConverter(new StepConverter())
                .WithTypeConverter(new VariableConverter())
                .WithTypeConverter(new VmImageConverter())
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            File.WriteAllText(ConfigurationFile, method!.IsStatic ?
                serializer.Serialize(method!.Invoke(null, new object[] { this, executableTargets })!) :
                serializer.Serialize(method!.Invoke(build, new object[] { this, executableTargets })!)
            );
        }

        public IEnumerable<Step> GetSteps(IEnumerable<IExecutableTarget> relevantTargets)
        {
            var steps = new List<Step>();
            foreach (IExecutableTarget executableTarget in relevantTargets)
            {
                var (partitionName, totalPartitions) = ArtifactExtensions.GetPartition(executableTarget.Definition);
                var publishedArtifacts = ArtifactExtensions.GetArtifactProducts(executableTarget.Definition)
                    // .Select(x => (AbsolutePath)x)
                    // .Select(x => x.DescendantsAndSelf(y => y.Parent).FirstOrDefault(y => !y.ToString().ContainsOrdinalIgnoreCase("*")))
                    .Distinct()
                    .Select(x => GetRelativePath(NukeBuild.RootDirectory, x))
                    // .Select(x => x.ToString().TrimStart(x.Parent.ToString()).TrimStart('/', '\\'))
                    .ToArray();

                // var artifactDependencies = (
                //     from artifactDependency in ArtifactExtensions.ArtifactDependencies[executableTarget.Definition]
                //     let dependency = executableTarget.ExecutionDependencies.Single(x => x.Factory == artifactDependency.Item1)
                //     let rules = (artifactDependency.Item2.Any()
                //             ? artifactDependency.Item2
                //             : ArtifactExtensions.ArtifactProducts[dependency.Definition])
                //         .Select(GetArtifactRule).ToArray()
                //     select new TeamCityArtifactDependency
                //            {
                //                BuildType = buildTypes[dependency].Single(x => x.Partition == null),
                //                ArtifactRules = rules
                //            }).ToArray<TeamCityDependency>();

                var invokedTargets = executableTarget
                    .DescendantsAndSelf(x => x.Triggers.Concat(x.ExecutionDependencies), x => NonEntryTargets.Contains(x.Name))
                    .Where(x => x == executableTarget || NonEntryTargets.Contains(x.Name))
                    .Reverse()
                    // order triggers after
                    .OrderBy(x => executableTarget.Triggers.Any(z => z == x))
                    .Select(x => x.Name)
                    .ToArray();

                var arguments = $"{invokedTargets.JoinSpace()} --skip";
                if (partitionName != null)
                    arguments += $" --{ParameterService.GetParameterDashedName(partitionName)} $(System.JobPositionInPhase)";

                steps.Add(new PwshTask()
                {
                    DisplayName = executableTarget.Name,
                    Inputs = new PwshTaskInputs()
                    {
                        TargetType = PwshTargetType.FilePath,
                        Arguments = arguments,
                        FilePath = "build.ps1",
                    }
                });

                // var dependencies = executableTarget
                //     .ExecutionDependencies
                //     .Where(x => !attribute.ExcludedTargets.Contains(x.Name) && !attribute.NonEntryTargets.Contains(x.Name))
                //     .ToArray();


            }
            return steps;
        }

        public IEnumerable<Job> GetJobs(IEnumerable<IExecutableTarget> relevantTargets, params Pool[] pools)
        {
            var jobs = new List<Job>();
            foreach (var pool in pools)
            {
                var jobPrefix = pool.Name ?? pool.VmImage.Name ?? pool.VmImage.ImageName;
                foreach (IExecutableTarget executableTarget in relevantTargets)
                {
                    var jobName = (!string.IsNullOrEmpty(jobPrefix) ? $"{jobPrefix}_{executableTarget.Name}" : executableTarget.Name).Replace(" ", "_").Replace("-", "_");
                    var jobDisplayName = !string.IsNullOrEmpty(jobPrefix) ? $"{jobPrefix} - {executableTarget.Name}" : executableTarget.Name;
                    var (partitionName, totalPartitions) = ArtifactExtensions.GetPartition(executableTarget.Definition);
                    var publishedArtifacts = ArtifactExtensions.GetArtifactProducts(executableTarget.Definition)
                        // .Select(x => (AbsolutePath)x)
                        // .Select(x => x.DescendantsAndSelf(y => y.Parent).FirstOrDefault(y => !y.ToString().ContainsOrdinalIgnoreCase("*")))
                        .Distinct()
                        .Select(x => GetRelativePath(NukeBuild.RootDirectory, x))
                        // .Select(x => x.ToString().TrimStart(x.Parent.ToString()).TrimStart('/', '\\'))
                        .ToArray();

                    // var artifactDependencies = (
                    //     from artifactDependency in ArtifactExtensions.ArtifactDependencies[executableTarget.Definition]
                    //     let dependency = executableTarget.ExecutionDependencies.Single(x => x.Factory == artifactDependency.Item1)
                    //     let rules = (artifactDependency.Item2.Any()
                    //             ? artifactDependency.Item2
                    //             : ArtifactExtensions.ArtifactProducts[dependency.Definition])
                    //         .Select(GetArtifactRule).ToArray()
                    //     select new TeamCityArtifactDependency
                    //            {
                    //                BuildType = buildTypes[dependency].Single(x => x.Partition == null),
                    //                ArtifactRules = rules
                    //            }).ToArray<TeamCityDependency>();

                    var invokedTargets = executableTarget
                        .DescendantsAndSelf(x => x.Triggers.Concat(x.ExecutionDependencies), x => NonEntryTargets.Contains(x.Name))
                        .Where(x => x == executableTarget || NonEntryTargets.Contains(x.Name))
                        .Reverse()
                        // order triggers after
                        .OrderBy(x => executableTarget.Triggers.Any(z => z == x))
                        .Select(x => x.Name)
                        .ToArray();

                    var dependencies = executableTarget
                        .ExecutionDependencies
                        .Where(x => !ExcludedTargets.Contains(x.Name) && !NonEntryTargets.Contains(x.Name))
                        .Select(x => (!string.IsNullOrEmpty(jobPrefix) ? $"{jobPrefix}_{x.Name}" : x.Name).Replace(" ", "_").Replace("-", "_"))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var arguments = $"{invokedTargets.JoinSpace()} --skip";
                    if (partitionName != null)
                        arguments += $" --{ParameterService.GetParameterDashedName(partitionName)} $(System.JobPositionInPhase)";


                    var job = new Job()
                    {
                        Name = jobName,
                        DisplayName = jobDisplayName,
                        DependsOn = dependencies,
                        Pool = pool,
                        Steps = {
                            new PwshTask()
                            {
                                DisplayName = executableTarget.Name,
                                Inputs = new PwshTaskInputs() {
                                    TargetType = PwshTargetType.FilePath,
                                    Arguments = arguments,
                                    FilePath = "build.ps1",
                                }
                            }
                        }
                    };
                    jobs.Add(job);

                    // jobs.Add(new Pwsh()
                    // {
                    //     DisplayName = executableTarget.Name,
                    //     Action = $".{System.IO.Path.DirectorySeparatorChar}build.ps1 {arguments}",
                    // });


                }
            }
            return jobs;
        }

        public IEnumerable<Stage> GetStages(IEnumerable<IExecutableTarget> relevantTargets, params Pool[] pools)
        {
            var stages = new List<Stage>();
            foreach (var pool in pools)
            {
                var jobs = new List<Job>();
                foreach (IExecutableTarget executableTarget in relevantTargets)
                {
                    var (partitionName, totalPartitions) = ArtifactExtensions.GetPartition(executableTarget.Definition);
                    var publishedArtifacts = ArtifactExtensions.GetArtifactProducts(executableTarget.Definition)
                        // .Select(x => (AbsolutePath)x)
                        // .Select(x => x.DescendantsAndSelf(y => y.Parent).FirstOrDefault(y => !y.ToString().ContainsOrdinalIgnoreCase("*")))
                        .Distinct()
                        .Select(x => GetRelativePath(NukeBuild.RootDirectory, x))
                        // .Select(x => x.ToString().TrimStart(x.Parent.ToString()).TrimStart('/', '\\'))
                        .ToArray();

                    // var artifactDependencies = (
                    //     from artifactDependency in ArtifactExtensions.ArtifactDependencies[executableTarget.Definition]
                    //     let dependency = executableTarget.ExecutionDependencies.Single(x => x.Factory == artifactDependency.Item1)
                    //     let rules = (artifactDependency.Item2.Any()
                    //             ? artifactDependency.Item2
                    //             : ArtifactExtensions.ArtifactProducts[dependency.Definition])
                    //         .Select(GetArtifactRule).ToArray()
                    //     select new TeamCityArtifactDependency
                    //            {
                    //                BuildType = buildTypes[dependency].Single(x => x.Partition == null),
                    //                ArtifactRules = rules
                    //            }).ToArray<TeamCityDependency>();

                    var invokedTargets = executableTarget
                        .DescendantsAndSelf(x => x.Triggers.Concat(x.ExecutionDependencies), x => NonEntryTargets.Contains(x.Name))
                        .Where(x => x == executableTarget || NonEntryTargets.Contains(x.Name))
                        .Reverse()
                        // order triggers after
                        .OrderBy(x => executableTarget.Triggers.Any(z => z == x))
                        .Select(x => x.Name)
                        .ToArray();

                    var dependencies = executableTarget
                        .ExecutionDependencies
                        .Where(x => !ExcludedTargets.Contains(x.Name) && !NonEntryTargets.Contains(x.Name))
                        .Select(x => x.Name)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var arguments = $"{invokedTargets.JoinSpace()} --skip";
                    if (partitionName != null)
                        arguments += $" --{ParameterService.GetParameterDashedName(partitionName)} $(System.JobPositionInPhase)";

                    var job = new Job()
                    {
                        Name = executableTarget.Name,
                        DependsOn = dependencies,
                        Pool = pool,
                        Steps = {
                            new PwshTask()
                            {
                                DisplayName = executableTarget.Name,
                                Inputs = new PwshTaskInputs() {
                                    TargetType = PwshTargetType.FilePath,
                                    Arguments = arguments,
                                    FilePath = "build.ps1"
                                }
                            }
                        }
                    };
                    jobs.Add(job);
                }
                stages.Add(new Stage()
                {
                    Name = pool.Name ?? pool.VmImage?.Name ?? "Nuke",
                    Jobs = jobs,
                });
            }
            return stages;
        }

    }
}

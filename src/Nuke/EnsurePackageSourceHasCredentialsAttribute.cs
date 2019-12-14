using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NuGet.Configuration;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Ensures that the package source name has credentials set
    /// This is useful to ensure that credentials are defined on a users local environment
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class EnsurePackageSourceHasCredentialsAttribute : Attribute, IOnBeforeLogo
    {
        /// <summary>
        /// Ensures that the package source name has credentials set
        /// This is useful to ensure that credentials are defined on a users local environment
        /// </summary>
        public EnsurePackageSourceHasCredentialsAttribute(string sourceName) => SourceName = sourceName;

        /// <summary>
        /// The nuget source name
        /// </summary>
        public string SourceName { get; }

        /// <inheritdoc />
        public void OnBeforeLogo(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
        {
            var settings = Settings.LoadDefaultSettings(NukeBuild.RootDirectory);
            var packageSourceProvider = new PackageSourceProvider(settings);

            var source = packageSourceProvider.LoadPackageSources()
               .FirstOrDefault(x => x.Name.Equals(SourceName, StringComparison.OrdinalIgnoreCase));
            if (source == null)
            {
                var error =
                    $"NuGet Package Source {SourceName} could not be found. This is required for the build to complete.";
                Logger.Error(error);
                throw new Exception(error);
            }

            if (source.Credentials?.IsValid() != true)
            {
                var error =
                    $"NuGet Package Source {SourceName} does not have any credentials defined.  Please configure the credentials for {SourceName} to build.";
                Logger.Error(error);
                throw new Exception(error);
            }
        }
    }

    [PublicAPI]
    public class AzurePipelinesStepsAttribute : ChainedConfigurationAttributeBase
    {
        private string ConfigurationFile => NukeBuild.RootDirectory / "azure-pipelines.nuke.yml";

        protected override HostType HostType => HostType.AzurePipelines;
        protected override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
        protected override IEnumerable<string> RelevantTargetNames => InvokedTargets;

        public string[] InvokedTargets { get; set; } = new string[0];
        public string[] Parameters { get; set; } = new string[0];

        protected override CustomFileWriter CreateWriter()
        {
            return new CustomFileWriter(ConfigurationFile, indentationFactor: 2, commentPrefix: "#");
        }

        protected override ConfigurationEntity GetConfiguration(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> relevantTargets
        )
        {
            var paramList = new List<AzurePipelinesParameter>();
            var parameters =
                build.GetType().GetMembers(
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public |
                        BindingFlags.FlattenHierarchy
                    )
                   .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null);
            foreach (var parameter in parameters)
            {
                Logger.Info(parameter.Name);
                if (Parameters.Any(
                    z => z.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase) || z.Equals(
                        parameter.GetCustomAttribute<ParameterAttribute>().Name,
                        StringComparison.OrdinalIgnoreCase
                    )
                ))
                {
                    var value = parameter.GetValue(build);
                    if (value is AbsolutePath)
                    {
                        value = null;
                    }

                    paramList.Add(
                        new AzurePipelinesParameter()
                        {
                            Name = parameter.GetCustomAttribute<ParameterAttribute>().Name ?? parameter.Name,
                            Default = value?.ToString() ?? "",
                        }
                    );
                }
            }

            var lookupTable = new LookupTable<ExecutableTarget, AzurePipelinesStep>();
            var steps = relevantTargets
               .Select(x => (ExecutableTarget: x, Job: GetStep(x, lookupTable)))
               .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Job))
               .Select(x => x.Job).ToArray();

            return new AzurePipelinesSteps()
            {
                Parameters = paramList.ToArray(),
                Steps = steps
            };
        }

        protected virtual AzurePipelinesStep GetStep(
            ExecutableTarget executableTarget,
            LookupTable<ExecutableTarget, AzurePipelinesStep> jobs
        )
        {
            // var (partitionName, totalPartitions) = ArtifactExtensions.Partitions.GetValueOrDefault(executableTarget.Definition);
            // var publishedArtifacts = ArtifactExtensions.ArtifactProducts[executableTarget.Definition]
            //     .Select(x => (PathConstruction.AbsolutePath) x)
            //     .Select(x => x.DescendantsAndSelf(y => y.Parent).FirstOrDefault(y => !y.ToString().ContainsOrdinalIgnoreCase("*")))
            //     .Distinct()
            //     .Select(x => x.ToString().TrimStart(x.Parent.ToString()).TrimStart('/', '\\')).ToArray();

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

            var chainLinkNames = GetInvokedTargets(executableTarget).ToArray();
            return new AzurePipelinesStep
            {
                Name = executableTarget.Name,
                DisplayName = executableTarget.Name,
                ScriptPath = PowerShellScript,
                InvokedTargets = chainLinkNames,
            };
        }
    }

    public class AzurePipelinesParameter : ConfigurationEntity
    {
        public string Name { get; set; }
        public string Default { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using var a = writer.WriteBlock($"{Name}: '{Default}'");
        }
    }

    public class AzurePipelinesSteps : ConfigurationEntity
    {
        public AzurePipelinesParameter[] Parameters { get; set; }
        public AzurePipelinesStep[] Steps { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            if (Parameters.Length > 0)
            {
                using (writer.WriteBlock($"parameters:"))
                {
                    foreach (var item in Parameters)
                    {
                        item.Write(writer);
                    }
                }
            }

            using (writer.WriteBlock($"steps:"))
            {
#pragma warning disable CA1308
                var parameters = Parameters.Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ parameters.{z.Name} }}}}'")
                   .ToArray()
                   .JoinSpace();
#pragma warning restore CA1308
                using (writer.WriteBlock($"- task: DotNetCoreCLI@2"))
                {
                    writer.WriteLine("displayName: 'install nuke'");
                    using (writer.WriteBlock("inputs:"))
                    {
                        writer.WriteLine("command: custom");
                        writer.WriteLine("custom: tool");
                        writer.WriteLine("arguments: 'upgrade -g Nuke.GlobalTool'");
                    }
                }

                foreach (var step in Steps)
                {
                    step.Write(writer, parameters);
                }
            }
        }
    }

    public class AzurePipelinesStep
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ScriptPath { get; set; }
        public string[] InvokedTargets { get; set; }

        public void Write(CustomFileWriter writer, string parameters)
        {
            using (writer.WriteBlock($"- script: nuke {InvokedTargets.JoinSpace()} --skip --no-logo {parameters}".TrimEnd()))
            {
                writer.WriteLine($"displayName: {DisplayName.SingleQuote()}");
            }
        }
    }
}
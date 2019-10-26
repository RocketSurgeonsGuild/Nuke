using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.AzurePipelines;
using Rocket.Surgery.Nuke.AzurePipelines.Configuration;
using System.Collections.Generic;
using Nuke.Common.CI;
using Nuke.Common.Utilities.Collections;
using System.Linq;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.Tooling;
using System;
using Nuke.Common.Utilities;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[RocketSurgeryAzurePipelines(
    nameof(AzureDevopsPipeline),
    ConfigurationFile = "azure-pipelines.yml",
    AutoGenerate = true,
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[] { nameof(DotnetToolRestore), nameof(Generate_Code_Coverage_Reports) },
    ExcludedTargets = new string[] { nameof(DotNetCore), nameof(Default), nameof(Clean), nameof(BuildVersion) }
)]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
class Build : DotNetCoreBuild
{
    /// <summary>
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>

    public static int Main() => Execute<Build>(x => x.Default);

    Target Default => _ => _.DependsOn(DotNetCore);

    static Pipeline AzureDevopsPipeline(RocketSurgeryAzurePipelinesAttribute attribute, IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        var relevantTargets = attribute.InvokedTargets
            .SelectMany(x => ExecutionPlanner.GetExecutionPlan(executableTargets, new[] { x }))
            .Distinct()
            .Where(x => !attribute.ExcludedTargets.Contains(x.Name) && !attribute.NonEntryTargets.Contains(x.Name)).ToList();

        var repo = new Repository
        {
            Identifier = "rsg",
            Type = RepositoryType.GitHub,
            Name = "RocketSurgeonsGuild/AzureDevopsTemplates",
            Ref = "refs/tags/v0.10.12",
            Endpoint = "github",
        };
        var steps = new[] {
                        new Task() {
                            DisplayName = "GitVersion",
                            Name = "gittools.gitversion.gitversion-task.GitVersion@5",
                            Inputs = {
                                ["configFilePath"] = "GitVersion.yml"
                            }
                        }
                    }.Concat(
                        attribute.GetSteps(relevantTargets)
                    ).ToList();
        var pipeline = new Pipeline()
        {
            Trigger = new Trigger()
            {
                Batch = true,
                Branches = new TriggerItem()
                {
                    Include = new List<string> { "master", "refs/tags/*" }
                },
                Paths = new TriggerItem()
                {
                    Exclude = new List<string> {
                        "**/*.md",
                        ".appveyor.yml",
                        ".codecov.yml",
                        "GitVersion.yml",
                        "GitReleaseManager.yaml",
                        ".vscode/*",
                        ".git*",
                        ".editorconfig",
                        ".nuke",
                        "LICENSE",
                    }
                }
            },
            PullRequest = new PullRequest()
            {
                AutoCancel = true,
                Branches = new TriggerItem()
                {
                    Include = new List<string> { "master" }
                }
            },
            Resources = new Resources() { Repositories = { repo } },
            Variables = new List<Variable> {
                new Variable {
                    Name = "CONFIGURATION",
                    Value = "Release",
                },
                new Variable {
                    Name = "VERBOSITY",
                    Value = "Normal",
                },
                new Variable {
                    Name = "COVERAGE",
                    Value = "$(Agent.BuildDirectory)/c",
                },
                new Variable {
                    Name = "ARTIFACTS",
                    Value = "$(Build.ArtifactStagingDirectory)",
                },
                new Variable {
                    Name = "DOTNET_SKIP_FIRST_TIME_EXPERIENCE",
                    Value = "true",
                },
                new Variable {
                    Name = "CodeCovToken",
                    Value = "c93f6719-da50-4d00-ba2b-b73fd95239e0",
                }
            },
            // Jobs = new List<Job>() {
            //     new Job() {

            //         Name = "GitVersion",
            //         DisplayName = "GitVersion",
            //         Pool = VmImage.UbuntuLatest,
            //         Steps = new [] {
            //             new Task() {
            //                 DisplayName = "GitVersion",
            //                 Name = "gittools.gitversion.gitversion-task.GitVersion@5",
            //                 Inputs = {
            //                     ["configFilePath"] = "GitVersion.yml"
            //                 }
            //             }
            //         }
            //     }
            // }.Concat(attribute.GetJobs(relevantTargets, VmImage.UbuntuLatest, VmImage.WindowsLatest).Select(j =>
            // {
            //     j.DependsOn.Add("GitVersion");
            //     return j;
            // })).ToList()
            Jobs = new List<Job>() {
                new Job {
                    Name = VmImage.WindowsLatest.Name,
                    Pool = VmImage.WindowsLatest,
                    Steps = steps
                },
                new Job {
                    Name = VmImage.UbuntuLatest.Name,
                    Pool = VmImage.UbuntuLatest,
                    Steps = steps
                },
                new Job {
                    Name = VmImage.MacOsLatest.Name,
                    Pool = VmImage.MacOsLatest,
                    Steps = steps
                }
            }
        };

        return pipeline;
    }
}

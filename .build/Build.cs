using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.DotNetCore;
using System;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using Rocket.Surgery.Nuke.GithubActions;
using System.Reflection;
using System.IO;
using GitHubActionsConfiguration = Rocket.Surgery.Nuke.GithubActions.GitHubActionsConfiguration;
using GitHubActionsUsingStep = Rocket.Surgery.Nuke.GithubActions.GitHubActionsUsingStep;
using GitHubActionsArtifactStep = Rocket.Surgery.Nuke.GithubActions.GitHubActionsArtifactStep;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AzurePipelinesSteps(
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[] { nameof(BuildVersion), nameof(Generate_Code_Coverage_Reports), nameof(Default), nameof(Clean) },
    ExcludedTargets = new[] { nameof(Restore), nameof(DotnetToolRestore), nameof(Clean) },
    Parameters = new[] { nameof(CoverageDirectory), nameof(ArtifactsDirectory), nameof(Verbosity), nameof(Configuration) }
)]
[GitHubActionsSteps("ci", GitHubActionsImage.MacOsLatest, GitHubActionsImage.WindowsLatest, GitHubActionsImage.UbuntuLatest,
    On = new[] { GitHubActionsTrigger.Push },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master", "du-fuk" },
    OnPullRequestBranches = new[] { "master", "du-fuk" },
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[] { nameof(BuildVersion), nameof(Generate_Code_Coverage_Reports), nameof(Default), nameof(Clean) },
    ExcludedTargets = new[] { nameof(Clean) },
    Enhancements = new[] { nameof(Middleware) }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
// [EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
[EnsureGitHooks(GitHook.PreCommit)]
internal class Solution : DotNetCoreBuild, IDotNetCoreBuild
{
    /// <summary>
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Solution>(x => x.Default);

    private Target Default => _ => _
       .DependsOn(Restore)
       .DependsOn(Build)
       .DependsOn(Test)
       .DependsOn(Pack);

    public Target Restore => _ => _.With(this, DotNetCoreBuild.Restore);

    public Target Build => _ => _.With(this, DotNetCoreBuild.Build);

    public Target Test => _ => _.With(this, DotNetCoreBuild.Test);

    public Target Pack => _ => _.With(this, DotNetCoreBuild.Pack);

    public static GitHubActionsConfiguration Middleware(GitHubActionsConfiguration configuration)
    {
        var buildJob = configuration.Jobs.First(z => z.Name == "Build");
        var checkoutStep = buildJob.Steps.First(z => z is GitHubActionsUsingStep step && step.Using == "actions/checkout@v1");
        buildJob.Steps.InsertRange(buildJob.Steps.IndexOf(checkoutStep) + 1, new[] {
            new GitHubActionsUsingStep
            {
                Name = "Install GitVersion",
                Using = "david-driscoll/gittools-actions/gitversion/setup@feature/export-environment-github",
                With = {
                    ["versionSpec"] = "5.1.x",
                }

            },
            new GitHubActionsUsingStep
            {
                Name = "Use GitVersion",
                Id = "gitversion",
                Using = "david-driscoll/gittools-actions/gitversion/execute@feature/export-environment-github"
            }
        });

        buildJob.Steps.Add(new GitHubActionsUsingStep()
        {
            Name = "Publish Coverage",
            Using = "codecov/codecov-action@v1",
            With = new Dictionary<string, string>
            {
                ["file"] = "${{ env.GITHUB_WORKSPACE }}/coverage/solution.xml",
                ["name"] = "actions-${{ matrix.os }}",
                ["fail_ci_if_error"] = "true",
            }
        });

        buildJob.Steps.Add(new GitHubActionsArtifactStep()
        {
            Name = "Publish logs",
            ArtifactName = "logs",
            Path = "artifacts/logs/",
            If = "always()"
        });

        buildJob.Steps.Add(new GitHubActionsArtifactStep()
        {
            Name = "Publish coverage data",
            ArtifactName = "coverage",
            Path = "coverage/",
            If = "always()"
        });

        buildJob.Steps.Add(new GitHubActionsArtifactStep()
        {
            Name = "Publish NuGet Packages",
            ArtifactName = "nuget",
            Path = "artifacts/nuget/",
            If = "always()"
        });


        /*

  - publish: "${{ parameters.Artifacts }}/logs/"
    displayName: Publish Logs
    artifact: "Logs${{ parameters.Postfix }}"
    condition: always()

  - publish: ${{ parameters.Coverage }}
    displayName: Publish Coverage
    artifact: "Coverage${{ parameters.Postfix }}"
    condition: always()

  - publish: "${{ parameters.Artifacts }}/nuget/"
    displayName: Publish NuGet Artifacts
    artifact: "NuGet${{ parameters.Postfix }}"
    condition: always()
        */
        return configuration;
    }
}

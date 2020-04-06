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
using RocketSurgeonGitHubActionsConfiguration = Rocket.Surgery.Nuke.GithubActions.RocketSurgeonGitHubActionsConfiguration;
using UsingStep = Rocket.Surgery.Nuke.GithubActions.UsingStep;
using UploadArtifactStep = Rocket.Surgery.Nuke.GithubActions.UploadArtifactStep;

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
    ExcludedTargets = new[] { nameof(DotnetToolRestore), nameof(Clean) },
    Enhancements = new[] { nameof(Middleware) }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
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

    public static RocketSurgeonGitHubActionsConfiguration Middleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        var buildJob = configuration.Jobs.First(z => z.Name == "Build");
        var checkoutStep = buildJob.Steps.OfType<CheckoutStep>().Single();
        // For fetch all
        // checkoutStep.FetchDepth = 0;
        buildJob.Steps.InsertRange(buildJob.Steps.IndexOf(checkoutStep) + 1, new BaseGitHubActionsStep[] {
            new RunStep("Fetch all history for all tags and branches") {
                Run = "git fetch --prune --unshallow"
            },
            new SetupDotNetStep("Use .NET Core 2.1 SDK") {
                DotNetVersion = "2.1.805"
            },
            new SetupDotNetStep("Use .NET Core 3.1 SDK") {
                DotNetVersion = "3.1.201"
            },
            new RunStep("nuget source") {
                Shell = GithubActionShell.Pwsh,
                Run = "dotnet nuget add source -n RocketSurgeonsGuild -u 'anything' -p ${{ secrets.RSG_PACKAGES_TOKEN }}",
            },
            new UsingStep("Install GitVersion")
            {
                Uses = "david-driscoll/gittools-actions/gitversion/setup@feature/export-environment-github",
                With = {
                    ["versionSpec"] = "5.1.x",
                }

            },
            new UsingStep("Use GitVersion")
            {
                Id = "gitversion",
                Uses = "david-driscoll/gittools-actions/gitversion/execute@feature/export-environment-github"
            },
        });

        buildJob.Steps.Add(new UsingStep("Publish Coverage")
        {
            Uses = "codecov/codecov-action@v1",
            With = new Dictionary<string, string>
            {
                ["name"] = "actions-${{ matrix.os }}",
                ["fail_ci_if_error"] = "true",
            }
        });

        buildJob.Steps.Add(new UploadArtifactStep("Publish logs")
        {
            Name = "logs",
            Path = "artifacts/logs/",
            If = "always()"
        });

        buildJob.Steps.Add(new UploadArtifactStep("Publish coverage data")
        {
            Name = "coverage",
            Path = "coverage/",
            If = "always()"
        });

        buildJob.Steps.Add(new UploadArtifactStep("Publish test data")
        {
            Name = "coverage",
            Path = "artifacts/test/",
            If = "always()"
        });

        buildJob.Steps.Add(new UploadArtifactStep("Publish NuGet Packages")
        {
            Name = "nuget",
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

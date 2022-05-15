using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.GithubActions;

#pragma warning disable CA1050

internal static class LocalConstants
{
    public static string[] PathsIgnore =
    {
        ".codecov.yml",
        ".editorconfig",
        ".gitattributes",
        ".gitignore",
        ".gitmodules",
        ".lintstagedrc.js",
        ".prettierignore",
        ".prettierrc",
        "LICENSE",
        "nukeeper.settings.json",
        "omnisharp.json",
        "package-lock.json",
        "package.json",
        "Readme.md",
        ".github/dependabot.yml",
        ".github/labels.yml",
        ".github/release.yml",
        ".github/renovate.json",
    };
}

[GitHubActionsSteps(
    "ci-ignore",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = new[] { GitHubActionsTrigger.Push },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master", "main", "next" },
    OnPullRequestBranches = new[] { "master", "main", "next" },
    Enhancements = new[] { nameof(CiIgnoreMiddleware) }
)]
[GitHubActionsSteps(
    "ci",
    GitHubActionsImage.MacOsLatest,
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = new[] { GitHubActionsTrigger.Push },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master", "main", "next" },
    OnPullRequestBranches = new[] { "master", "main", "next" },
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[]
    {
        nameof(ICIEnvironment.CIEnvironment),
        nameof(ITriggerCodeCoverageReports.TriggerCodeCoverageReports),
        nameof(ITriggerCodeCoverageReports.GenerateCodeCoverageReportCobertura),
        nameof(IGenerateCodeCoverageBadges.GenerateCodeCoverageBadges),
        nameof(IGenerateCodeCoverageReport.GenerateCodeCoverageReport),
        nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary),
        nameof(Default)
    },
    ExcludedTargets = new[] { nameof(ICanClean.Clean), nameof(ICanRestoreWithDotNetCore.DotnetToolRestore) },
    Enhancements = new[] { nameof(CiMiddleware) }
)]
[PrintBuildVersion]
[PrintCIEnvironment]
[UploadLogs]
[TitleEvents]
[ContinuousIntegrationConventions]
public partial class Solution
{
    public static RocketSurgeonGitHubActionsConfiguration CiIgnoreMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration
    )
    {
        foreach (var item in configuration.DetailedTriggers.OfType<RocketSurgeonGitHubActionsVcsTrigger>())
        {
            item.IncludePaths = LocalConstants.PathsIgnore;
        }

        configuration.Jobs.RemoveAt(1);
        ( (RocketSurgeonsGithubActionsJob)configuration.Jobs[0] ).Steps = new List<GitHubActionsStep>
        {
            new RunStep("N/A")
            {
                Run = "echo \"No build required\""
            }
        };

        return configuration;
    }

    public static RocketSurgeonGitHubActionsConfiguration CiMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration
    )
    {
        foreach (var item in configuration.DetailedTriggers.OfType<RocketSurgeonGitHubActionsVcsTrigger>())
        {
            item.ExcludePaths = LocalConstants.PathsIgnore;
        }

        configuration.Jobs.OfType<RocketSurgeonsGithubActionsJob>()
                     .First(z => z.Name == "Build")
                     .UseDotNetSdks("3.1", "6.0")
                     .AddNuGetCache()
                      // .ConfigureForGitVersion()
                     .ConfigureStep<CheckoutStep>(step => step.FetchDepth = 0)
                     .PublishLogs<Solution>()
                     .FailFast = false;

        return configuration;
    }
}

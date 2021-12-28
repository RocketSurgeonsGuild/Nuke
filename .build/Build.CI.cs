using Newtonsoft.Json;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.GithubActions;
using YamlDotNet.Core;

#pragma warning disable CA1050

[GitHubActionsSteps(
    "ci",
    GitHubActionsImage.MacOsLatest,
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
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
    Enhancements = new[] { nameof(Middleware) }
)]
[PrintBuildVersion]
[PrintCIEnvironment]
[UploadLogs]
public partial class Solution
{
    public static RocketSurgeonGitHubActionsConfiguration Middleware(
        RocketSurgeonGitHubActionsConfiguration configuration
    )
    {
        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("check_ignore_paths")
            {
                Images = new[] { GitHubActionsImage.UbuntuLatest },
                Outputs =
                {
                    ["should_skip"] = "${{ steps.skip_check.outputs.should_skip }}",
                },
                Steps = new List<GitHubActionsStep>()
                {
                    new UsingStep("Check ignore-paths")
                    {
                        Uses = "fkirc/skip-duplicate-actions@v3.4.1",
                        With =
                        {
                            ["paths_ignore"] = JsonConvert.SerializeObject(new[]
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
                                    "Readme.md"
                                }
                            ),
                        }
                    }
                }
            }
        );
        /*
         - name: Skip Duplicate Actions
  uses: fkirc/skip-duplicate-actions@v3.4.1

         */
        // paths_ignore: '["**/README.md", "**/docs/**"]'
        var buildJob = configuration.Jobs.OfType<RocketSurgeonsGithubActionsJob>().First(z => z.Name == "Build");
        buildJob.Needs.Add("check_ignore_paths");
        buildJob.FailFast = false;
        buildJob.If = "${{ needs.check_ignore_paths.outputs.should_skip != 'true' }}";
        var checkoutStep = buildJob.Steps.OfType<CheckoutStep>().Single();
        // For fetch all
        checkoutStep.FetchDepth = 0;
        buildJob.Steps.InsertRange(
            buildJob.Steps.IndexOf(checkoutStep) + 1,
            new BaseGitHubActionsStep[]
            {
                new RunStep("Fetch all history for all tags and branches")
                {
                    Run = "git fetch --prune"
                },
                new SetupDotNetStep("Use .NET Core 2.1 SDK")
                {
                    DotNetVersion = "2.1.x"
                },
                new SetupDotNetStep("Use .NET Core 3.1 SDK")
                {
                    DotNetVersion = "3.1.x"
                },
                new SetupDotNetStep("Use .NET Core 5.0 SDK")
                {
                    DotNetVersion = "5.0.x"
                },
                new SetupDotNetStep("Use .NET Core 6.0 SDK")
                {
                    DotNetVersion = "6.0.x"
                },
            }
        );

        buildJob.Steps.Add(
            new UsingStep("Publish Coverage")
            {
                Uses = "codecov/codecov-action@v1",
                With = new Dictionary<string, string>
                {
                    ["name"] = "actions-${{ matrix.os }}",
                }
            }
        );

        buildJob.Steps.Add(
            new UploadArtifactStep("Publish logs")
            {
                Name = "logs",
                Path = "artifacts/logs/",
                If = "always()"
            }
        );

        buildJob.Steps.Add(
            new UploadArtifactStep("Publish coverage data")
            {
                Name = "coverage",
                Path = "coverage/",
                If = "always()"
            }
        );

        buildJob.Steps.Add(
            new UploadArtifactStep("Publish test data")
            {
                Name = "test data",
                Path = "artifacts/test/",
                If = "always()"
            }
        );

        buildJob.Steps.Add(
            new UploadArtifactStep("Publish NuGet Packages")
            {
                Name = "nuget",
                Path = "artifacts/nuget/",
                If = "always()"
            }
        );

        return configuration;
    }
}

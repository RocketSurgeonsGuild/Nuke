using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Adds update milestone support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class UpdateMilestoneJobAttribute() : GitHubActionsStepsAttribute("update-milestone", GitHubActionsImage.UbuntuLatest)
{
    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Update Milestone"
        };
        build.DetailedTriggers.Add(new RocketSurgeonGitHubActionsWorkflowTrigger());
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.PullRequestTarget,
                Types = ["closed", "opened", "reopened", "synchronize"],
            }
        );
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("update_milestone")
            {
                RunsOn = ( !IsGithubHosted ) ? Images : [],
                Matrix = IsGithubHosted ? Images : [],
                Steps =
                [
                    new CheckoutStep("Checkout")
                    {
                        FetchDepth = 0,
                    },
                    new RunStep("Fetch all history for all tags and branches")
                    {
                        Run = "git fetch --prune",
                    },
                    new SetupDotNetStep("Install DotNet"),
                    new RunStep("dotnet tool restore")
                    {
                        Run = "dotnet tool restore",
                    },
                    new UsingStep("Install GitVersion")
                    {
                        Uses = "gittools/actions/gitversion/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitVersion.Tool").Version
                        },
                    },
                    new UsingStep("Install GitReleaseManager")
                    {
                        Uses = "gittools/actions/gitreleasemanager/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitReleaseManager.Tool").Version
                        },
                    },
                    new UsingStep("Use GitVersion")
                    {
                        Id = "gitversion",
                        Uses = "gittools/actions/gitversion/execute@v3.1.1",
                        With =
                        {
                            ["useConfigFile"] = "true",
                        }
                    },
                    new UsingStep("Create Milestone")
                    {
                        Uses = "WyriHaximus/github-action-create-milestone@v1",
                        With =
                        {
                            ["title"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}"
                        },
                        Environment =
                        {
                            ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                        ContinueOnError = true,
                    },
                    new UsingStep("sync milestones")
                    {
                        Uses = "RocketSurgeonsGuild/actions/sync-milestone@v0.3.15",
                        With =
                        {
                            ["default-label"] = ":sparkles: mysterious",
                            ["github-token"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                    },
                ]
            }
        );

        return build;
    }
}

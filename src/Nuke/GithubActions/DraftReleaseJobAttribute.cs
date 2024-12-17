using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Adds draft release support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class DraftReleaseJobAttribute() : GitHubActionsStepsAttribute("draft-release", GitHubActionsImage.UbuntuLatest)
{
    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Draft Release and Create Milestone"
        };
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowCall,
                Secrets = [new GitHubActionsSecret("RSG_BOT_TOKEN", Required: true)],
            }
        );
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.Push,
                Branches = ["master"],
                ExcludePaths = ["**/*.md"],
            }
        );
        build.DetailedTriggers.Add(new GitHubActionsScheduledTrigger { Cron = "0 0 * * 4" });
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("draft_release")
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
                    new UsingStep("Install GitVersion")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "gittools/actions/gitversion/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitVersion.Tool").Version
                        },
                    },
                    new UsingStep("Install GitReleaseManager")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "gittools/actions/gitreleasemanager/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitReleaseManager.Tool").Version
                        },
                    },
                    new UsingStep("Create Milestone")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "WyriHaximus/github-action-create-milestone@v1",
                        With =
                        {
                            ["title"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}"
                        },
                        Environment =
                        {
                            ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                    },
                    new UsingStep("sync milestones")
                    {
                        Uses = "RocketSurgeonsGuild/actions/sync-milestone@v0.3.15",
                        With =
                        {
                            ["default-label"] = ":sparkles: mysterious",
                            ["github-token"] = "${{ secrets.GITHUB_TOKEN }"
                        }
                    },

                    new UsingStep("Create Release")
                    {
                        Uses = "ncipollo/release-action@v1",
                        With =
                        {
                            ["allowUpdates"] = "true", ["generateReleaseNotes"] = "true",
                            ["draft"] = "true",
                            ["omitNameDuringUpdate"] = "true",
                            ["name"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                            ["tag"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                            ["token"] = "${{ secrets.RSG_BOT_TOKEN }}",
                            ["commit"] = "${{ github.base_ref }}"
                        },
                    },
                ]
            }
        );

        return build;
    }
}

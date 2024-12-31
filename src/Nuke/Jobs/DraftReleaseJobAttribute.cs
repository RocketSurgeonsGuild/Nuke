using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke.Jobs;

/// <summary>
///     Adds draft release support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class DraftReleaseJobAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public DraftReleaseJobAttribute() : base("draft-release", GitHubActionsImage.UbuntuLatest) => AutoGenerate = false;

    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public DraftReleaseJobAttribute(string image, params string[] images) : base("draft-release", image, images) => AutoGenerate = false;

    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public DraftReleaseJobAttribute(GitHubActionsImage image) : base("draft-release", image) => AutoGenerate = false;

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Draft Release and Create Milestone",
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
        var job = this.CreateJob(
            "draft_release",
            true,
            [
                ..WorkflowHelpers.RunGitVersion(),
                ..WorkflowHelpers.CreateMilestone(),
                ..WorkflowHelpers.SyncMilestones(),
                new UsingStep("Create Release")
                {
                    Uses = "ncipollo/release-action@v1",
                    With =
                    {
                        ["allowUpdates"] = "true",
                        ["generateReleaseNotes"] = "true",
                        ["updateOnlyUnreleased"] = "true",
                        ["draft"] = "true",
                        ["omitBodyDuringUpdate"] = "true",
                        ["omitNameDuringUpdate"] = "true",
                        ["omitDraftDuringUpdate"] = "true",
                        ["name"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                        ["tag"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                        ["token"] = "${{ secrets.RSG_BOT_TOKEN }}",
                        ["commit"] = "${{ github.base_ref }}",
                    },
                },
            ]
        );
        build.Jobs.Add(job);

        return build;
    }
}

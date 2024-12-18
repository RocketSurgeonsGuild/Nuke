using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke.Jobs;

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
        var job = this.CreateJob(
            "update_milestone",
            true,
            [
                ..WorkflowHelpers.RunGitVersion(),
                ..WorkflowHelpers.CreateMilestone(),
                ..WorkflowHelpers.SyncMilestones(),
            ]
        );
        build.Jobs.Add(job);

        return build;
    }
}

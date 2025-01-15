using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;

using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke.Jobs;

/// <summary>
///     Adds update milestone support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class UpdateMilestoneJobAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    ///     Adds update milestone support to the build
    /// </summary>
    public UpdateMilestoneJobAttribute() : base("update-milestone", GitHubActionsImage.UbuntuLatest) => AutoGenerate = false;

    /// <summary>
    ///     Adds update milestone support to the build
    /// </summary>
    public UpdateMilestoneJobAttribute(string image, params string[] images) : base("update-milestone", image, images) => AutoGenerate = false;

    /// <summary>
    ///     Adds update milestone support to the build
    /// </summary>
    public UpdateMilestoneJobAttribute(GitHubActionsImage image) : base("update-milestone", image) => AutoGenerate = false;

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Update Milestone",
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
        NormalizeActionVersions(build);

        return build;
    }
}

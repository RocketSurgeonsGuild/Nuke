using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

[PublicAPI]
public enum RocketSurgeonGitHubActionsTrigger
{
    [EnumValue("push")] Push,
    [EnumValue("pull_request")] PullRequest,
    [EnumValue("workflow_dispatch")] WorkflowDispatch,
    [EnumValue("workflow_call")] WorkflowCall
}

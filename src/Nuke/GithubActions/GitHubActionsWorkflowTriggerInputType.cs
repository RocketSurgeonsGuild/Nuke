using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

public enum GitHubActionsWorkflowTriggerInputType
{
    [EnumValue("string")] String,
    [EnumValue("boolean")] Boolean,
    [EnumValue("number")] Number,
}

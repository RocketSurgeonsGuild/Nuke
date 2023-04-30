using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

public enum GitHubActionsInputType
{
    [EnumValue("string")] String,
    [EnumValue("boolean")] Boolean,
    [EnumValue("number")] Number,
}

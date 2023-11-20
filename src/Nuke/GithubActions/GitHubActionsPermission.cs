using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Github actions permissions
/// </summary>
public enum GitHubActionsPermission
{
    /// <summary>
    /// None
    /// </summary>
    [EnumValue("none")] None = 0,

    /// <summary>
    /// Read
    /// </summary>
    [EnumValue("read")] Read = 1,

    /// <summary>
    /// Write
    /// </summary>
    [EnumValue("write")] Write = 2,
}

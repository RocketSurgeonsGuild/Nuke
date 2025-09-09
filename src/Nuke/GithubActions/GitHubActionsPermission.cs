using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Github actions permissions
/// </summary>
#pragma warning disable CA1711
public enum GitHubActionsPermission
#pragma warning restore CA1711
{
    /// <summary>
    ///     None
    /// </summary>
    [EnumValue("none")]
    None = 0,

    /// <summary>
    ///     Read
    /// </summary>
    [EnumValue("read")]
    Read = 1,

    /// <summary>
    ///     Write
    /// </summary>
    [EnumValue("write")]
    Write = 2,
}

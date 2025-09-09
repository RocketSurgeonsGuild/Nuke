using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The input types
/// </summary>
public enum GitHubActionsInputType
{
    /// <summary>
    ///     String input
    /// </summary>
#pragma warning disable CA1720
    [EnumValue("string")]
    String,
#pragma warning restore CA1720
    /// <summary>
    ///     Boolean input
    /// </summary>
    [EnumValue("boolean")]
    Boolean,

    /// <summary>
    ///     Number input
    /// </summary>
    [EnumValue("number")]
    Number,
}

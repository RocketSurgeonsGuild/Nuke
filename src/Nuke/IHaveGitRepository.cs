using Nuke.Common.Git;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines use of a git repository
/// </summary>
/// <remarks>
///     This explicitly excludes the attribute so that it can be defined in the consumers build
/// </remarks>
public interface IHaveGitRepository : IHave
{
    /// <summary>
    ///     The Git Repository currently being built
    /// </summary>
    GitRepository? GitRepository { get; }
}

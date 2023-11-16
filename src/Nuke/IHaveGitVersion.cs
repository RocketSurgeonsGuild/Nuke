using Nuke.Common.Tools.GitVersion;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines use of GitVersion
/// </summary>
public interface IHaveGitVersion : IHave
{
    /// <summary>
    ///     The current version as defined by GitVersion
    /// </summary>
    [ComputedGitVersion]
    public GitVersion GitVersion => null!;
}

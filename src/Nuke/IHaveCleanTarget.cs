using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a clean target
/// </summary>
public interface IHaveCleanTarget : IHave
{
    /// <summary>
    ///     The Clean Target
    /// </summary>
    [ExcludeTarget]
    Target Clean { get; }
}
/// <summary>
///     Defines a clean target
/// </summary>
public interface IHaveLintTarget : IHave
{
    /// <summary>
    ///     The Lint Target
    /// </summary>
    Target Lint { get; }
}

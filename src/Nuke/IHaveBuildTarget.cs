namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the build target
/// </summary>
public interface IHaveBuildTarget : IHave
{
    /// <summary>
    ///     The Build Target
    /// </summary>
    Target Build { get; }
}
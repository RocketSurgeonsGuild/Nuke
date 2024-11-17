namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a clean target
/// </summary>
public interface IHaveLintTarget : IHave
{
    /// <summary>
    ///     The Lint Target
    /// </summary>
    [NonEntryTarget]
    Target Lint { get; }
}

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a clean target
/// </summary>
public interface IHaveCleanTarget : IHave
{
    /// <summary>
    ///     The Clean Target
    /// </summary>
    Target Clean { get; }
}

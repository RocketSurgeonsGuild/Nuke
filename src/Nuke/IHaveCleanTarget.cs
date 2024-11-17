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

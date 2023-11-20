namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the pack target
/// </summary>
public interface IHavePackTarget : IHave
{
    /// <summary>
    ///     The Pack Target
    /// </summary>
    Target Pack { get; }
}

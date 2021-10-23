namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the test target
/// </summary>
public interface IHaveTestTarget : IHave
{
    /// <summary>
    ///     The Test Target
    /// </summary>
    Target Test { get; }
}

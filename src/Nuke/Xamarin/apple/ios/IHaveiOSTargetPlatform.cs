// ReSharper disable once CheckNamespace
// ReSharper disable InconsistentNaming

namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Targeting an ios platform
/// </summary>
public interface IHaveiOSTargetPlatform : IHave
{
    /// <summary>
    ///     Gets the target platform.
    /// </summary>
    /// <value>The target platform.</value>
    [Parameter("The target platform for iOS")]
    TargetPlatform iOSTargetPlatform { get; }
}

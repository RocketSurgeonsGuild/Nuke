// ReSharper disable once CheckNamespace

namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Has a bundle identifier.
/// </summary>
public interface IHaveBundleIdentifier : IHave
{
    /// <summary>
    ///     Gets the path for the info plist.
    /// </summary>
    [Parameter("The application bundle identifier.", Name = "BundleIdentifier")]
    public string BundleIdentifier => EnvironmentInfo.GetVariable<string>("BundleIdentifier")
     ?? TryGetValue(() => BundleIdentifier)
     ?? "com.surgery.rocket.nuke";

    /// <summary>
    ///     Gets the suffix for the bundle identifier.
    /// </summary>
    [Parameter("The identifier suffix.")]
    public string Suffix { get; set; }
}

using Nuke.Common.IO;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Has an info.plist file.
/// </summary>
public interface IHaveInfoPlist : IHave
{
    /// <summary>
    ///     Gets the path for the info plist.
    /// </summary>
    [Parameter("The path to the info.plist.")]
    public AbsolutePath InfoPlist { get; }
}

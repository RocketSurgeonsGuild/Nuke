// ReSharper disable once CheckNamespace

namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Base build plan for Xamarin Android based applications
/// </summary>
public interface IXamarinAndroidBuild : INukeBuild
{
    /// <summary>
    ///     Core target that can be used to trigger all targets for this build
    /// </summary>
    Target XamarinAndroid { get; }
}

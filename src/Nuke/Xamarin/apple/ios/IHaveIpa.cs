using Nuke.Common.IO;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Defines the ipa output directory.
/// </summary>
public interface IHaveIpa : IHaveArtifacts
{
    /// <summary>
    ///     The directory where the ipa is to be dropped.
    /// </summary>
    [Parameter("The directory where artifacts are to be dropped", Name = "Ipa")]
    AbsolutePath IpaDirectory => EnvironmentInfo.GetVariable<AbsolutePath>("Ipa")
     ?? TryGetValue(() => IpaDirectory)
     ?? NukeBuild.RootDirectory / "artifacts" / "ios";
}

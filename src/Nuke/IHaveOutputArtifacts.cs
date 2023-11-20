using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the output directory
/// </summary>
public interface IHaveOutputArtifacts : IHaveArtifacts
{
    /// <summary>
    ///     The directory where packaged output should be placed (zip, webdeploy, etc)
    /// </summary>
    public AbsolutePath OutputArtifactsDirectory => ArtifactsDirectory / "output";
}

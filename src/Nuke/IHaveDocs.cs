using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Interface for a build that has documentation generation
/// </summary>
public interface IHaveDocs : IHaveArtifacts
{
    /// <summary>
    ///     Where the docs are stored
    /// </summary>
    public AbsolutePath DocumentationDirectory => NukeBuild.RootDirectory / "docs";

    /// <summary>
    ///     Where the docs are output
    /// </summary>
    public AbsolutePath DocumentationsOutputDirectory => ArtifactsDirectory / "docs";
}

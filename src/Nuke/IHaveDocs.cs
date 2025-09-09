using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Interface for a build that has documentation generation
/// </summary>
[PublicAPI]
public interface IHaveDocs : IHaveArtifacts
{
    /// <summary>
    ///     The Build Target
    /// </summary>
    Target Docs { get; }

    /// <summary>
    ///     Where the docs are stored
    /// </summary>
    AbsolutePath DocumentationDirectory => NukeBuild.RootDirectory / "docs";

    /// <summary>
    ///     Where the docs are output
    /// </summary>
    AbsolutePath DocumentationsOutputDirectory => ArtifactsDirectory / "docs";
}

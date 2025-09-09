using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     The directory where sources should be placed.
/// </summary>
public interface IComprehendSources : IComprehend
{
    /// <summary>
    ///     The directory where samples will be placed
    /// </summary>
    AbsolutePath SourceDirectory => FilePathExtensions.PickDirectory(
        NukeBuild.RootDirectory / "src",
        NukeBuild.RootDirectory / "source",
        NukeBuild.RootDirectory / "sources"
    );
}

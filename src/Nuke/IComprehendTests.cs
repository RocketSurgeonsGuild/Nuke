using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     The directory where tests should be placed.
/// </summary>
public interface IComprehendTests : IComprehend
{
    /// <summary>
    ///     The directory where tests will be placed
    /// </summary>
    public AbsolutePath TestsDirectory => FilePathExtensions.PickDirectory(
        NukeBuild.RootDirectory / "test",
        NukeBuild.RootDirectory / "tests"
    );
}

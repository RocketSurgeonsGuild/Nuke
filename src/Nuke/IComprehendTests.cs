using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     The directory where tests should be placed.
/// </summary>
public interface IComprehendTests : IComprehend
{
    /// <summary>
    /// The build configuration to use for testing
    /// </summary>
    string TestBuildConfiguration => "Debug";

    /// <summary>
    ///     The directory where tests will be placed
    /// </summary>
    AbsolutePath TestsDirectory => FilePathExtensions.PickDirectory(NukeBuild.RootDirectory / "test", NukeBuild.RootDirectory / "tests");

    /// <summary>
    ///     The default path to look for user (eg. commited to the repo) test runsettings
    /// </summary>
    AbsolutePath RunSettings => FilePathExtensions.PickFile(
        TestsDirectory / "settings.runsettings",
        TestsDirectory / "tests.runsettings",
        TestsDirectory / "coverlet.runsettings"
    ).ExistingFile() ?? NukeBuild.TemporaryDirectory / "default.runsettings";
}

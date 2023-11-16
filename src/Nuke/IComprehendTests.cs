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
    public AbsolutePath TestsDirectory => FilePathExtensions.PickDirectory(NukeBuild.RootDirectory / "test", NukeBuild.RootDirectory / "tests");
    
    /// <summary>
    /// The default path to look for user (eg. commited to the repo) test runsettings
    /// </summary>
    public AbsolutePath RunSettings => FilePathExtensions.PickFile(TestsDirectory / "settings.runsettings", TestsDirectory / "tests.runsettings", TestsDirectory / "coverlet.runsettings");
}

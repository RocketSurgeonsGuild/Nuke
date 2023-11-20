using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the test result artifacts locations
/// </summary>
/// <remarks>
///     Used for things like xunit test result files for publish to azure devops or otherwise.
/// </remarks>
public interface IHaveTestArtifacts : IHaveArtifacts
{
    /// <summary>
    ///     The directory where test results will be placed
    /// </summary>
    public AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test";
}

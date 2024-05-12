using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds a code coverage directory
/// </summary>
/// <remarks>
///     This directory is left separate to allow easier integration with editors that might look it's contents to display
///     coverage.
/// </remarks>
public interface IHaveCodeCoverage : IHaveArtifacts
{
    /// <summary>
    ///     The directory where coverage artifacts are to be dropped
    /// </summary>
    [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
    public AbsolutePath CoverageDirectory => EnvironmentInfo.GetVariable<AbsolutePath>("Coverage")
     ?? TryGetValue(() => CoverageDirectory)
     ?? NukeBuild.RootDirectory / "coverage";
}
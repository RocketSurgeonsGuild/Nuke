using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a build version target
/// </summary>
[PublicAPI]
public interface IHaveBuildVersion : IHaveGitVersion, IHaveSolution, IHaveConfiguration
{
    /// <summary>
    ///     prints the build information.
    /// </summary>
    public Target BuildVersion => d => d
                                     .Executes(
                                          () =>
                                          {
                                              Log.Information(
                                                  "Building version {NuGetVersion} of {SolutionName} ({Configuration}) using version {NukeVersion} of Nuke",
                                                  GitVersion.NuGetVersionV2 ?? GitVersion.NuGetVersion,
                                                  Solution.Name,
                                                  Configuration,
                                                  typeof(NukeBuild).Assembly.GetVersionText()
                                              );
                                          }
                                      );
}

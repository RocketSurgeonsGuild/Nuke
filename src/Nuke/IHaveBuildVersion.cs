using Rocket.Surgery.Nuke.GithubActions;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a build version target
/// </summary>
public interface IHaveBuildVersion : IHaveGitVersion, IHaveSolution, IHaveConfiguration
{
    /// <summary>
    ///     prints the build information.
    /// </summary>
    [NonEntryTarget]
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

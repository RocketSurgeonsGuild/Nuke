namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a build version target
/// </summary>
public interface IHaveBuildVersion : IHaveGitVersion, IHaveSolution, IHaveConfiguration
{
    /// <summary>
    ///     prints the build information.
    /// </summary>
    public Target BuildVersion => _ => _
       .Executes(
            () =>
            {
                Logger.Info(
                    "Building version {0} of {1} ({2}) using version {3} of Nuke.",
                    GitVersion?.NuGetVersionV2 ?? GitVersion?.NuGetVersion,
                    Solution.Name,
                    Configuration,
                    typeof(NukeBuild).Assembly.GetVersionText()
                );
            }
        );
}

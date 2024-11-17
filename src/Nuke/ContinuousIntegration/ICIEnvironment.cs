using Serilog;

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

// ReSharper disable InconsistentNaming
/// <summary>
///     Dumps ci state for debug purposes
/// </summary>
[PublicAPI]
public interface ICIEnvironment : IHaveBuildVersion
{
    /// <summary>
    ///     Well know environment variables
    /// </summary>
    /// <remarks>
    ///     Replace default implementation to add values not covered by default
    /// </remarks>
    public IEnumerable<string> WellKnownEnvironmentVariablePrefixes => new[]
    {
        // Azure pipelines
        "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS", "BITRISE", "BAMBOO", "GITLAB", "JENKINS", "TEAMCITY",
        "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
    };

    /// <summary>
    ///     Prints CI environment state for debug purposes
    /// </summary>
    [NonEntryTarget]
    public Target CIEnvironment => d => d
                                       .Unlisted()
                                       .OnlyWhenStatic(() => NukeBuild.IsServerBuild)
                                       .Executes(
                                            () =>
                                            {
                                                Log.Information("CI: {CI}", EnvironmentInfo.GetVariable<string>("CI"));

                                                foreach (var variable in WellKnownEnvironmentVariablePrefixes
                                                            .SelectMany(
                                                                 prefix => EnvironmentInfo.Variables.Keys.Where(
                                                                     key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                                                                 )
                                                             ))
                                                {
                                                    Log.Information("{Key}: {Value}", variable, EnvironmentInfo.Variables[variable]);
                                                }
                                            }
                                        );
}

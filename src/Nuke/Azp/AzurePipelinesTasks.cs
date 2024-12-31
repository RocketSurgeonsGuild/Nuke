using Nuke.Common.CI.AzurePipelines;

#pragma warning disable CA1822
// ReSharper disable MissingBaseTypeHighlighting

namespace Rocket.Surgery.Nuke.Azp;

/// <summary>
///     Base pipeline build task
/// </summary>
// ReSharper disable once MissingBaseTypeHighlighting
public class AzurePipelinesTasks
{
    /// <summary>
    ///     Gets a value that determines if the build is running on Azure DevOps.
    /// </summary>
    public static Func<bool> IsRunningOnAzurePipelines => () =>
                                                              NukeBuild.Host is AzurePipelines || Environment.GetEnvironmentVariable("LOGNAME") == "vsts";

    /// <summary>
    ///     Gets a value that determines if the build is not running on Azure DevOps.
    /// </summary>
    public static Func<bool> IsNotRunningOnAzurePipelines => () =>
                                                                 !( NukeBuild.Host is AzurePipelines
                                                                  || Environment.GetEnvironmentVariable("LOGNAME") == "vsts" );

    /// <summary>
    ///     Publish the test results
    /// </summary>
    private Target PublishAzurePipelinesTestResults => d => d
                                                           .Before(PublishAzurePipelinesCodeCoverage)
                                                           .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                           .Executes(() => { });

    /// <summary>
    ///     Publish the code coverage
    /// </summary>
    private Target PublishAzurePipelinesCodeCoverage => d => d
                                                            .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                            .Executes(() => { });
}

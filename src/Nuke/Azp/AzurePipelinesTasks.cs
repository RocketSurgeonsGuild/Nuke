using System.Linq.Expressions;
using Nuke.Common.CI.AzurePipelines;
using static Nuke.Common.Logger;
using static Nuke.Common.EnvironmentInfo;

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
    public static Expression<Func<bool>> IsRunningOnAzurePipelines => () =>
        NukeBuild.Host is AzurePipelines || Environment.GetEnvironmentVariable("LOGNAME") == "vsts";

    /// <summary>
    ///     Print the azure pipelines environment
    /// </summary>
    private Target PrintAzurePipelinesEnvironment => _ => _
                                                         .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                         .Executes(
                                                              () =>
                                                              {
                                                                  Info("AGENT_ID: {0}", GetVariable<string>("AGENT_ID"));
                                                                  Info("AGENT_NAME: {0}", GetVariable<string>("AGENT_NAME"));
                                                                  Info("AGENT_VERSION: {0}", GetVariable<string>(" "));
                                                                  Info("AGENT_JOBNAME: {0}", GetVariable<string>("AGENT_JOBNAME"));
                                                                  Info("AGENT_JOBSTATUS: {0}", GetVariable<string>("AGENT_JOBSTATUS"));
                                                                  Info("AGENT_MACHINE_NAME: {0}", GetVariable<string>("AGENT_MACHINE_NAME"));
                                                                  Info("\n");

                                                                  Info("BUILD_BUILDID: {0}", GetVariable<string>("BUILD_BUILDID"));
                                                                  Info("BUILD_BUILDNUMBER: {0}", GetVariable<string>("BUILD_BUILDNUMBER"));
                                                                  Info("BUILD_DEFINITIONNAME: {0}", GetVariable<string>("BUILD_DEFINITIONNAME"));
                                                                  Info("BUILD_DEFINITIONVERSION: {0}", GetVariable<string>("BUILD_DEFINITIONVERSION"));
                                                                  Info("BUILD_QUEUEDBY: {0}", GetVariable<string>("BUILD_QUEUEDBY"));
                                                                  Info("\n");

                                                                  Info("BUILD_SOURCEBRANCHNAME: {0}", GetVariable<string>("BUILD_SOURCEBRANCHNAME"));
                                                                  Info("BUILD_SOURCEVERSION: {0}", GetVariable<string>("BUILD_SOURCEVERSION"));
                                                                  Info("BUILD_REPOSITORY_NAME: {0}", GetVariable<string>("BUILD_REPOSITORY_NAME"));
                                                                  Info("BUILD_REPOSITORY_PROVIDER: {0}", GetVariable<string>("BUILD_REPOSITORY_PROVIDER"));
                                                              }
                                                          );

    /// <summary>
    ///     Upload the artifacts when running in azure pipelines
    /// </summary>
    private Target UploadAzurePipelinesArtifacts => _ => _
                                                        .Before(PublishAzurePipelinesTestResults)
                                                        .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                        .Executes(() => { });

    /// <summary>
    ///     Publish the test results
    /// </summary>
    private Target PublishAzurePipelinesTestResults => _ => _
                                                           .Before(PublishAzurePipelinesCodeCoverage)
                                                           .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                           .Executes(() => { });

    /// <summary>
    ///     Publish the code coverage
    /// </summary>
    private Target PublishAzurePipelinesCodeCoverage => _ => _
                                                            .OnlyWhenStatic(IsRunningOnAzurePipelines)
                                                            .Executes(() => { });

    /// <summary>
    ///     Run the azure pipelines targets
    /// </summary>
    [UsedImplicitly]
    private Target AzurePipelines => _ => _
                                         .DependsOn(PrintAzurePipelinesEnvironment)
                                         .DependsOn(UploadAzurePipelinesArtifacts)
                                         .DependsOn(PublishAzurePipelinesTestResults)
                                         .DependsOn(PublishAzurePipelinesCodeCoverage);
}

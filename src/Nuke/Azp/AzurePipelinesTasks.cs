using System.Linq.Expressions;
using Nuke.Common.CI.AzurePipelines;
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
                                                                  Serilog.Log.Information("AGENT_ID: {AgentID}", GetVariable<string>("AGENT_ID"));
                                                                  Serilog.Log.Information("AGENT_NAME: {AgentName}", GetVariable<string>("AGENT_NAME"));
                                                                  Serilog.Log.Information("AGENT_VERSION: {AgentVersion}", GetVariable<string>(" "));
                                                                  Serilog.Log.Information("AGENT_JOBNAME: {AgentJobName}", GetVariable<string>("AGENT_JOBNAME"));
                                                                  Serilog.Log.Information("AGENT_JOBSTATUS: {AgentJobStatus}", GetVariable<string>("AGENT_JOBSTATUS"));
                                                                  Serilog.Log.Information("AGENT_MACHINE_NAME: {AgentMachineName}", GetVariable<string>("AGENT_MACHINE_NAME"));
                                                                  Serilog.Log.Information("\n");

                                                                  Serilog.Log.Information("BUILD_BUILDID: {BuildBuildId}", GetVariable<string>("BUILD_BUILDID"));
                                                                  Serilog.Log.Information("BUILD_BUILDNUMBER: {BuildBuildnumber}", GetVariable<string>("BUILD_BUILDNUMBER"));
                                                                  Serilog.Log.Information("BUILD_DEFINITIONNAME: {BuildDefinitionName}", GetVariable<string>("BUILD_DEFINITIONNAME"));
                                                                  Serilog.Log.Information("BUILD_DEFINITIONVERSION: {BuildDefinitionVersion}", GetVariable<string>("BUILD_DEFINITIONVERSION"));
                                                                  Serilog.Log.Information("BUILD_QUEUEDBY: {BuildQueuedBy}", GetVariable<string>("BUILD_QUEUEDBY"));
                                                                  Serilog.Log.Information("\n");

                                                                  Serilog.Log.Information("BUILD_SOURCEBRANCHNAME: {BuildSourceBranchName}", GetVariable<string>("BUILD_SOURCEBRANCHNAME"));
                                                                  Serilog.Log.Information("BUILD_SOURCEVERSION: {BuildSourceVersion}", GetVariable<string>("BUILD_SOURCEVERSION"));
                                                                  Serilog.Log.Information("BUILD_REPOSITORY_NAME: {BuildRepositoryName}", GetVariable<string>("BUILD_REPOSITORY_NAME"));
                                                                  Serilog.Log.Information("BUILD_REPOSITORY_PROVIDER: {BuildRepositoryProvider}", GetVariable<string>("BUILD_REPOSITORY_PROVIDER"));
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

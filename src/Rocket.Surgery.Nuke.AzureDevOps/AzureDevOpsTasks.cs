using System;
using System.Linq.Expressions;
using Nuke.Common;
using static Nuke.Common.Logger;
using static Nuke.Common.EnvironmentInfo;
#pragma warning disable CA1822

namespace Rocket.Surgery.Nuke.AzurePipelines
{
    /// <summary>
    /// Base pipeline build task
    /// </summary>
    public class AzurePipelinesTasks
    {
        /// <summary>
        /// Gets a value that determines if the build is running on Azure DevOps.
        /// </summary>
        public static Expression<Func<bool>> IsRunningOnAzurePipelines => () =>
            NukeBuild.Host == HostType.AzurePipelines || Environment.GetEnvironmentVariable("LOGNAME") == "vsts";

        private Target PrintAzurePipelinesEnvironment => _ => _
           .OnlyWhenStatic(IsRunningOnAzurePipelines)
           .Executes(
                () =>
                {
                    Info("AGENT_ID: {0}", GetVariable<string>("AGENT_ID"));
                    Info("AGENT_NAME: {0}", GetVariable<string>("AGENT_NAME"));
                    Info("AGENT_VERSION: {0}", GetVariable<string>("AGENT_VERSION"));
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

        private Target UploadAzurePipelinesArtifacts => _ => _
           .Before(PublishAzurePipelinesTestResults)
           .OnlyWhenStatic(IsRunningOnAzurePipelines)
           .Executes(() => { });

        private Target PublishAzurePipelinesTestResults => _ => _
           .Before(PublishAzurePipelinesCodeCoverage)
           .OnlyWhenStatic(IsRunningOnAzurePipelines)
           .Executes(() => { });

        private Target PublishAzurePipelinesCodeCoverage => _ => _
           .OnlyWhenStatic(IsRunningOnAzurePipelines)
           .Executes(() => { });

        private Target AzurePipelines => _ => _
           .DependsOn(PrintAzurePipelinesEnvironment)
           .DependsOn(UploadAzurePipelinesArtifacts)
           .DependsOn(PublishAzurePipelinesTestResults)
           .DependsOn(PublishAzurePipelinesCodeCoverage);
    }
}
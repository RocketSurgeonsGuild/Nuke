using System;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class UploadLogsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
    {
        /// <inheritdoc />
        public void OnBuildFinished(NukeBuild build)
        {
            if (build is IHaveOutputLogs logs)
            {
                foreach (var item in logs.LogsDirectory.GlobFiles("**/*"))
                {
                    UploadFile(item);
                }
            }
        }

        void UploadFile(AbsolutePath path)
        {
            AzurePipelines.Instance?.WriteCommand("task.uploadfile", path);
        }
    }
}
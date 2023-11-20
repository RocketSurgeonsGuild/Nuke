using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.IO;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Automagically upload logs in supported environments
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class UploadLogsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnBuildFinished()
    {
        if (Build is IHaveOutputLogs logs)
        {
            foreach (var item in logs.LogsDirectory.GlobFiles("**/*"))
            {
                UploadFile(item);
            }
        }
    }

    private static void UploadFile(AbsolutePath path)
    {
        AzurePipelines.Instance?.WriteCommand("task.uploadfile", path);
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;
}

using Nuke.Common.Execution;
using Serilog;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Print the build version out
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class PrintBuildVersionAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    /// <inheritdoc />
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (Build is IHaveGitVersion gitVersion && Build is IHaveSolution solution &&
            Build is IHaveConfiguration configuration)
        {
            Log.Information(
                "Building version {InformationalVersion} of {SolutionName} ({Configuration}) using version {NukeVersion} of Nuke",
                gitVersion.GitVersion?.InformationalVersion,
                solution.Solution.Name,
                configuration.Configuration,
                typeof(NukeBuild).Assembly.GetVersionText()
            );
        }
    }
}

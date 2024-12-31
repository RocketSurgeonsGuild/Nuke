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
        if (Build is not (IHaveGitVersion gitVersion and IHaveSolution solution and IHaveConfiguration configuration)) return;

        Log.Logger.Information(
            "Building version {InformationalVersion} of {SolutionName} ({Configuration}) using version {NukeVersion} of Nuke",
            gitVersion.GitVersion.InformationalVersion,
            solution.Solution.Name,
            configuration.Configuration,
            typeof(NukeBuild).Assembly.GetVersionText()
        );
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;
}

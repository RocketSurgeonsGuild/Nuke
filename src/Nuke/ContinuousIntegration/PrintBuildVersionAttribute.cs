using Nuke.Common.Execution;
using static Nuke.Common.Logger;

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
        NukeBuild build,
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (build is IHaveGitVersion gitVersion && build is IHaveSolution solution &&
            build is IHaveConfiguration configuration)
        {
            using (Block("Build Version"))
            {
                Info(
                    "Building version {0} of {1} ({2}) using version {3} of Nuke.",
                    gitVersion.GitVersion?.InformationalVersion,
                    solution.Solution.Name,
                    configuration.Configuration,
                    typeof(NukeBuild).Assembly.GetVersionText()
                );
            }
        }
    }
}

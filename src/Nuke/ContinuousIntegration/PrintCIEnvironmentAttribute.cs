using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;
using Serilog;

// ReSharper disable InconsistentNaming

#pragma warning disable CA1019
namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Print ci environment with additional variables
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class PrintCIEnvironmentAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    private readonly string[] _additionalPrefixes;

    /// <summary>
    ///     Print ci environment with additional variables
    /// </summary>
    /// <param name="additionalPrefixes"></param>
    public PrintCIEnvironmentAttribute(params string[] additionalPrefixes)
    {
        _additionalPrefixes = additionalPrefixes;
    }

    /// <summary>
    ///     Well know environment variables
    /// </summary>
    /// <remarks>
    ///     Replace default implementation to add values not covered by default
    /// </remarks>
    private static string[] WellKnownEnvironmentVariablePrefixes => new[]
    {
        // Azure pipelines
        "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS", "BITRISE", "BAMBOO", "GITLAB", "JENKINS", "TEAMCITY",
        "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
    };

    /// <inheritdoc />
    public void OnBuildInitialized(
        NukeBuild build,
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (NukeBuild.IsLocalBuild)
            return;

        Log.Information("CI: {CI}", EnvironmentInfo.GetVariable<string>("CI"));

        foreach (var variable in WellKnownEnvironmentVariablePrefixes
                                .Concat(_additionalPrefixes)
                                .SelectMany(
                                     prefix => EnvironmentInfo.Variables.Keys.Where(
                                         key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                                     )
                                 ))
        {
            Log.Information("{Key}: {Value}", variable, EnvironmentInfo.Variables[variable]);
        }
    }
}

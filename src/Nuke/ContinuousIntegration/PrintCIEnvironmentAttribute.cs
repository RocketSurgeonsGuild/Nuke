using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitLab;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;
using Serilog;

// ReSharper disable InconsistentNaming

#pragma warning disable CA1019
namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Print ci environment with additional variables
/// </summary>
/// <remarks>
///     Print ci environment with additional variables
/// </remarks>
/// <param name="additionalPrefixes"></param>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class PrintCIEnvironmentAttribute(params string[] additionalPrefixes) : BuildExtensionAttributeBase, IOnBuildInitialized, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnBuildFinished()
    {
        if (Build.GetType().HasCustomAttribute<ContinuousIntegrationConventionsAttribute>()) return;

        Log.Logger.Warning("Please updated the build to also be decorated with [ContinuousIntegrationConventions]");
        // Shim in compatibility such that this "just works" until the build is updated to use the new attribute
        new ContinuousIntegrationConventionsAttribute().OnBuildFinished();
    }

    /// <inheritdoc />
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (NukeBuild.IsLocalBuild)
            return;

        GitHubActions.Instance?.Group("PrintCIEnvironment");
        AzurePipelines.Instance?.Group("PrintCIEnvironment");
        GitLab.Instance?.BeginSection("PrintCIEnvironment");
        TeamCity.Instance?.OpenBlock("PrintCIEnvironment");

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

        TeamCity.Instance?.CloseBlock("PrintCIEnvironment");
        GitLab.Instance?.EndSection("PrintCIEnvironment");
        AzurePipelines.Instance?.EndGroup("PrintCIEnvironment");
        GitHubActions.Instance?.EndGroup("PrintCIEnvironment");
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;

    private readonly string[] _additionalPrefixes = additionalPrefixes;

    /// <summary>
    ///     Well know environment variables
    /// </summary>
    /// <remarks>
    ///     Replace default implementation to add values not covered by default
    /// </remarks>
    private static string[] WellKnownEnvironmentVariablePrefixes =>
    [
        // Azure pipelines
        "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS", "BITRISE", "BAMBOO", "GITLAB", "JENKINS", "TEAMCITY",
        "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
    ];
}

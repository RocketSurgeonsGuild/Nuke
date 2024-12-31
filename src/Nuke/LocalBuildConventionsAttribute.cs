using Nuke.Common.Execution;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Attribute is used for conventions that will only run on a local build
///     Used an extension point to ensure the local build environment is configured correctly.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LocalBuildConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished, IOnBuildInitialized, IOnBuildCreated
{
    /// <inheritdoc />
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets) => Log.Logger = new LoggerConfiguration()
                                                                                                       .ConfigureEnricher()
                                                                                                       .ConfigureConsole(null)
                                                                                                       .ConfigureLevel()
                                                                                                       .CreateLogger();

    /// <inheritdoc />
    public void OnBuildFinished()
    {
        //        if (Build is not ({ } nukeBuild and IHaveSolution)) return;
        //        if (nukeBuild.IsServerBuild) return;
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        //        if (Build is not (INukeBuild nukeBuild and IHaveSolution haveSolution)) return;
        //        if (nukeBuild.IsServerBuild) return;
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;
}

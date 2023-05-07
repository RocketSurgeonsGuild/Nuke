using Nuke.Common.Execution;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Sets titles on the console as the build happens locally
/// </summary>
public sealed class TitleEventsAttribute : BuildExtensionAttributeBase, IOnBuildCreated, IOnBuildInitialized, IOnTargetRunning, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnTargetRunning(ExecutableTarget target)
    {
        Console.Title = $"Nuke :: {Symbols.StepName(target.Name)}";
    }

    /// <inheritdoc />
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        Console.Title = "Nuke :: 🗽";
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        Console.Title = "Nuke :: 🌟";
    }

    /// <inheritdoc />
    public void OnBuildFinished()
    {
        Console.Title = "Nuke :: 🗡";
        if (Build.GetType().HasCustomAttribute<LocalBuildConventionsAttribute>()) return;

        Log.Logger.Warning("Please updated the build to also be decorated with [LocalBuildConventions]");
        // Shim in compatibility such that this "just works" until the build is updated to use the new attribute
        new LocalBuildConventionsAttribute().OnBuildFinished();
    }
}

using Nuke.Common.Execution;

using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Sets titles on the console as the build happens locally
/// </summary>
public sealed class TitleEventsAttribute : BuildExtensionAttributeBase, IOnBuildCreated, IOnBuildInitialized, IOnTargetRunning, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        Console.Title = "Nuke :: 🗽";
        ProgressBar(ProgressBarState.Indeterminate);
    }

    /// <inheritdoc />
    public void OnBuildFinished()
    {
        Console.Title = "Nuke :: 🗡";
        ProgressBar(ProgressBarState.None);
        if (Build.GetType().HasCustomAttribute<LocalBuildConventionsAttribute>()) return;

        Log.Logger.Warning("Please updated the build to also be decorated with [LocalBuildConventions]");
        // Shim in compatibility such that this "just works" until the build is updated to use the new attribute
        new LocalBuildConventionsAttribute().OnBuildFinished();
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        Console.Title = "Nuke :: 🌟";
        _plan = executionPlan;
        ProgressBar(ProgressBarState.Default, ProgressPercent(_plan, ref step));
    }

    /// <inheritdoc />
    public void OnTargetRunning(ExecutableTarget target)
    {
        Console.Title = $"Nuke :: {Symbols.StepName(target.Name)}";
        ProgressBar(ProgressBarState.Default, ProgressPercent(_plan, ref step));
    }

    private enum ProgressBarState
    {
        None = 0,
        Default = 1,

        //        Error = 2,
        Indeterminate = 3,
        //        Warning = 4
    }

    private static void ProgressBar(ProgressBarState state, short progress = 0)
    {
        if (NukeBuild.IsServerBuild) return;
        Console.Write($"\x1b]9;4;{state};{progress}\x07");
    }

    private static short ProgressPercent(IReadOnlyCollection<ExecutableTarget> plan, ref int step)
    {
        if (NukeBuild.IsServerBuild) return 0;
        var total = plan.Count + 1;

/* Unmerged change from project 'Rocket.Surgery.Nuke(net9.0)'
Before:
        return (short)Math.Round(( (double)step++ / total ) * 100);
After:
        return (short)Math.Round((double)step++ / total * 100);
*/
        return (short)Math.Round( (double)step++ / total  * 100);
    }

    private IReadOnlyCollection<ExecutableTarget> _plan = [];
    private int step;
}

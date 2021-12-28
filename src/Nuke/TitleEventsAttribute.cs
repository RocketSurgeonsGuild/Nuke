using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke;

public class TitleEventsAttribute : BuildExtensionAttributeBase, IOnBuildCreated, IOnBuildInitialized, IOnTargetRunning, IOnBuildFinished
{
    public void OnTargetRunning(NukeBuild build, ExecutableTarget target)
    {
        Console.Title =$"Nuke :: {Symbols.StepName(target.Name)}";
    }

    public void OnBuildCreated(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        Console.Title =$"Nuke :: 🗽";
    }

    public void OnBuildInitialized(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        Console.Title =$"Nuke :: 🌟";
    }

    public void OnBuildFinished(NukeBuild build)
    {
        Console.Title =$"Nuke :: 🗡";
    }
}
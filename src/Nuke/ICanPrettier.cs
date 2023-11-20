using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines targets for projects that use preitter
/// </summary>
public interface ICanPrettier : ICanLint
{
    /// <summary>
    ///     The prettier target
    /// </summary>
    public Target Prettier => d =>
        d
           .DependentFor(PostLint)
           .After(Lint)
           .Executes(() => ProcessTasks.StartProcess(ToolPathResolver.GetPathExecutable("npx"), "prettier . --write").AssertWaitForExit().AssertZeroExitCode());
}

using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Adds a task to run `dotnet build` with structured logging and gitversion configuration
/// </summary>
[PublicAPI]
public interface ICanBuildWithDotNetCore : IHaveRestoreTarget,
    IHaveConfiguration,
    IHaveBuildTarget,
    IHaveSolution,
    IHaveOutputLogs,
    IHaveGitVersion,
    ICan
{
    /// <summary>
    ///     dotnet build
    /// </summary>
    public Target DotNetCoreBuild => d => d
                                         .Description("Builds all the projects.")
                                         .Unlisted()
                                         .TryDependentFor<IHaveBuildTarget>(a => a.Build)
                                         .TryAfter<IHaveRestoreTarget>(a => a.Restore)
                                         .Executes(
                                              () => DotNetTasks.DotNetBuild(
                                                  s => s
                                                      .SetProjectFile(Solution)
                                                      .SetDefaultLoggers(LogsDirectory / "build.log")
                                                      .SetGitVersionEnvironment(GitVersion)
                                                      .SetConfiguration(Configuration)
                                                      .EnableNoRestore()
                                              )
                                          );
}

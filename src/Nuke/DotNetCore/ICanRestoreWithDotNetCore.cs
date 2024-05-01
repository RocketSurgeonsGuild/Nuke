using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Adds a task for `dotnet tool restore` and `dotnet restore`
/// </summary>
public interface ICanRestoreWithDotNetCore : IHaveCleanTarget,
    IHaveSolution,
    IHaveOutputLogs,
    IHaveGitVersion,
    IHaveRestoreTarget,
    ICan
{
    /// <summary>
    ///     This will ensure that all local dotnet tools are installed
    /// </summary>
    public Target DotnetToolRestore => d => d
                                           .After(Clean)
                                           .OnlyWhenStatic(() => ( NukeBuild.RootDirectory / ".config" / "dotnet-tools.json" ).FileExists())
                                           .Unlisted()
                                           .Executes(() => DotNet($"tool restore"));

    /// <summary>
    ///     This will ensure that all local dotnet tools are installed
    /// </summary>
    public Target DotnetWorkloadRestore => d => d
                                               .After(Clean)
                                               .Unlisted()
                                               .Executes(() => DotNet($"workload restore"));

    /// <summary>
    ///     dotnet restore
    /// </summary>
    public Target CoreRestore => d => d
                                     .Description("Restores the dependencies.")
                                     .Unlisted()
                                     .After(Clean)
                                     .DependsOn(DotnetToolRestore)
                                     .DependsOn(DotnetWorkloadRestore)
                                     .Executes(
                                          () => DotNetRestore(
                                              s => s
                                                  .SetProjectFile(Solution)
                                                  .EnableDisableParallel()
                                                  .SetDefaultLoggers(LogsDirectory / "restore.log")
                                                  .SetGitVersionEnvironment(GitVersion)
                                          )
                                      );
}
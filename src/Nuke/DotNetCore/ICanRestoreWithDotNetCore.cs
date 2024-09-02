using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Rocket.Surgery.Nuke.GithubActions;
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
    [ExcludeTarget]
    [NonEntryTarget]
    public Target DotNetToolRestore => d => d
                                           .Unlisted()
                                           .After(Clean)
                                           .TryDependentFor<IHaveRestoreTarget>(a => a.Restore)
                                           .OnlyWhenStatic(() => ( NukeBuild.RootDirectory / ".config" / "dotnet-tools.json" ).FileExists())
                                           .Executes(() => DotNet($"tool restore"));

    /// <summary>
    ///     dotnet restore
    /// </summary>
    [NonEntryTarget]
    public Target DotNetCoreRestore => d => d
                                           .Description("Restores the dependencies.")
                                           .Unlisted()
                                           .TryDependentFor<IHaveRestoreTarget>(a => a.Restore)
                                           .After(Clean)
                                           .DependsOn(DotNetToolRestore)
                                           .Executes(
                                                () => DotNetRestore(
                                                    s => s
                                                        .SetProjectFile(Solution)
                                                        .SetDefaultLoggers(LogsDirectory / "restore.log")
                                                        .SetGitVersionEnvironment(GitVersion)
                                                )
                                            );
}

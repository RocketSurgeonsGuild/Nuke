using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Adds a task for `dotnet tool restore` and `dotnet restore`
/// </summary>
[PublicAPI]
public interface ICanRestoreWithDotNetCore : IHaveCleanTarget,
    IHaveSolution,
    IHaveOutputLogs,
    IHaveGitVersion,
    IHaveRestoreTarget,
    ICan
{
    /// <summary>
    ///     dotnet restore
    /// </summary>
    [NonEntryTarget]
    Target DotnetCoreRestore => d => d
                                               .Description("Restores the dependencies.")
                                               .Unlisted()
                                               .TryDependentFor<IHaveRestoreTarget>(a => a.Restore)
                                               .After(Clean)
                                               .DependsOn(DotnetToolRestore)
                                               .Net9MsBuildFix()
                                               .Executes(
                                                    () => DotNetRestore(
                                                        s => s
                                                            .SetProcessWorkingDirectory(RootDirectory)
                                                            .SetProjectFile(Solution)
                                                            .SetDefaultLoggers(LogsDirectory / "restore.log")
                                                            .SetGitVersionEnvironment(GitVersion)
                                                    )
                                                );

    /// <summary>
    ///     This will ensure that all local dotnet tools are installed
    /// </summary>
    [ExcludeTarget]
    [NonEntryTarget]
    Target DotnetToolRestore => d => d
                                               .Unlisted()
                                               .After(Clean)
                                               .TryDependentFor<IHaveRestoreTarget>(a => a.Restore)
                                               .OnlyWhenStatic(() => ( NukeBuild.RootDirectory / ".config" / "dotnet-tools.json" ).FileExists())
                                               .Executes(() => DotNet("tool restore", RootDirectory));
}

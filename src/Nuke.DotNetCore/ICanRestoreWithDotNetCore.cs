using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Adds a task for `dotnet tool restore` and `dotnet restore`
    /// </summary>
    public interface ICanRestoreWithDotNetCore : IHaveCleanTarget,
                                              IHaveSolution,
                                              IHaveOutputLogs,
                                              IHaveGitVersion,
                                              IHaveRestoreTarget
    {
        /// <summary>
        /// This will ensure that all local dotnet tools are installed
        /// </summary>
        public Target DotnetToolRestore => _ => _
           .After(Clean)
           .OnlyWhenStatic(() => FileExists(NukeBuild.RootDirectory / ".config/dotnet-tools.json"))
           .Unlisted()
           .Executes(() => DotNet("tool restore"));

        /// <summary>
        /// dotnet restore
        /// </summary>
        public Target CoreRestore => _ => _
           .Description("Restores the dependencies.")
           .Unlisted()
           .After(Clean)
           .DependsOn(DotnetToolRestore)
           .Executes(
                () => DotNetRestore(
                    s => s
                       .SetProjectFile(Solution)
                       .SetDisableParallel(true)
                       .SetDefaultLoggers(LogsDirectory / "restore.log")
                       .SetGitVersionEnvironment(GitVersion)
                )
            );
    }
}
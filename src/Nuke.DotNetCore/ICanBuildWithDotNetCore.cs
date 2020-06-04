using Nuke.Common;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Adds a task to run `dotnet build` with structured logging and gitversion configuration
    /// </summary>
    public interface ICanBuildWithDotNetCore : IHaveRestoreTarget,
                                            IHaveConfiguration,
                                            IHaveBuildTarget,
                                            IHaveSolution,
                                            IHaveOutputLogs,
                                            IHaveGitVersion
    {
        /// <summary>
        /// dotnet build
        /// </summary>
        public Target CoreBuild => _ => _
           .Description("Builds all the projects.")
           .DependsOn(Restore)
           .Executes(
                () => DotNetTasks.DotNetBuild(
                    s => DotNetBuildSettingsExtensions.SetProjectFile<DotNetBuildSettings>(s, (string)Solution)
                       .SetDefaultLoggers(LogsDirectory / "build.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetConfiguration(Configuration)
                       .EnableNoRestore()
                )
            );
    }
}
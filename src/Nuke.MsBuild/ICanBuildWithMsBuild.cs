using Nuke.Common;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Defines a build task using msbuild
    /// </summary>
    public interface ICanBuildWithMsBuild : IHaveBuildTarget, IHaveRestoreTarget, IHaveSolution, IHaveConfiguration, IHaveOutputLogs, IHaveGitVersion
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target NetBuild => _ => _
           .DependsOn(Restore)
           .Executes(
                () => MSBuildTasks.MSBuild(
                    settings =>
                        settings
                           .SetSolutionFile(Solution)
                           .SetConfiguration(Configuration)
                           .SetDefaultLoggers(LogsDirectory / "build.log")
                           .SetGitVersionEnvironment(GitVersion)
                           .SetAssemblyVersion(GitVersion.AssemblySemVer)
                           .SetPackageVersion(GitVersion.NuGetVersionV2)
                )
            );
    }
}
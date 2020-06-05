using Nuke.Common;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// xamarin build with MSBuild
    /// </summary>
    public interface ICanBuildXamarin : IHaveRestoreTarget,
                                        IHaveSolution,
                                        IHaveConfiguration,
                                        IHaveGitVersion,
                                        IHaveOutputLogs
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
           .DependsOn(Restore)
           .Executes(
                () => MSBuild(
                    settings =>
                        settings.SetSolutionFile(Solution)
                           .SetTargetPlatform(MSBuildTargetPlatform.x64)
                           .SetConfiguration(Configuration)
                           .SetDefaultLoggers(LogsDirectory / "build.log")
                           .SetGitVersionEnvironment(GitVersion)
                           .SetAssemblyVersion(GitVersion.AssemblySemVer)
                           .SetPackageVersion(GitVersion.NuGetVersionV2)
                )
            );
    }
}
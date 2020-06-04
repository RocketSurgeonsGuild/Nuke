using Nuke.Common;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface ICanBuildXamarin : IHaveRestoreTarget, IHaveSolution, IHaveConfiguration, IHaveGitVersion, IHaveOutputLogs
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
           .DependsOn(Restore)
           .Executes(() => MSBuildTasks.MSBuild(settings =>
                MSBuildSettingsExtensions.SetSolutionFile((MSBuildSettings)settings, (string)Solution)
                   .SetTargetPlatform(MSBuildTargetPlatform.x64)
                   .SetConfiguration(Configuration)
                   .SetDefaultLoggers(LogsDirectory / "build.log")
                   .SetGitVersionEnvironment(GitVersion)
                   .SetAssemblyVersion(GitVersion.AssemblySemVer)
                   .SetPackageVersion(GitVersion.NuGetVersionV2)));
    }
}
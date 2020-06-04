using Nuke.Common;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface IHavePackXamarinMac : IHavePackTarget,
                                       IHaveTestTarget,
                                       IHaveConfiguration,
                                       IHaveOutputLogs,
                                       IHaveGitVersion,
                                       IHaveSolution
    {
        /// <summary>
        /// packages a binary for distribution.
        /// </summary>
        public Target Package => _ => _
           .DependsOn(Test)
           .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
           .Executes(
                () => MSBuildTasks.MSBuild(
                    settings => MSBuildSettingsExtensions.SetSolutionFile((MSBuildSettings)settings, (string)Solution)
                       .SetProperty("Platform", TargetPlatform.AnyCPU)
                       .SetProperty("BuildIpa", "true")
                       .SetProperty("ArchiveOnBuild", "true")
                       .SetConfiguration(Configuration)
                       .SetDefaultLoggers(LogsDirectory / "package.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetAssemblyVersion(GitVersion.AssemblySemVer)
                       .SetPackageVersion(GitVersion.NuGetVersionV2)
                )
            );
    }
}
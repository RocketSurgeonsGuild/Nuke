using Nuke.Common;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Xamarin iOS Pack
    /// </summary>
    public interface ICanPackXamariniOS : IHavePackTarget,
                                          IHaveTestTarget,
                                          IHaveConfiguration,
                                          IHaveOutputLogs,
                                          IHaveGitVersion,
                                          IHaveSolution,
                                          IHaveiOSTargetPlatform
    {
        /// <summary>
        /// packages a binary for distribution.
        /// </summary>
        public Target PackiPhone => _ => _
           .DependsOn(Test)
           .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
           .Executes(
                () => MSBuild(
                    settings => settings.SetSolutionFile(Solution)
                       .SetProperty("Platform", iOSTargetPlatform)
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
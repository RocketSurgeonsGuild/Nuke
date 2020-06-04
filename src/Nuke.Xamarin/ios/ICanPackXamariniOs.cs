using Nuke.Common;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface ICanPackXamariniOs : IHavePackTarget, IHaveTestTarget, IHaveConfiguration, IHaveOutputLogs, IHaveGitVersion, IHaveSolution, IHaveiOSTargetPlatform
    {
        /// <summary>
        /// packages a binary for distribution.
        /// </summary>
        public Target PackiPhone => _ => _
           .DependsOn(Test)
           .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
           .Executes(
                () => MSBuildTasks.MSBuild(
                    settings => MSBuildSettingsExtensions.SetSolutionFile((MSBuildSettings)settings, (string)Solution)
                       .SetProperty("Platform", iOSTargetPlatform)
                       .SetProperty("BuildIpa", "true")
                       .SetProperty("ArchiveOnBuild", "true")
                       .SetConfiguration(Configuration)
                       .SetDefaultLoggers(LogsDirectory / "package.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetAssemblyVersion(GitVersion.AssemblySemVer)
                       .SetPackageVersion(GitVersion.NuGetVersionV2)));
    }
}
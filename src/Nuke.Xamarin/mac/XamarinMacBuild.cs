using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface IBuildXamarinMac : IHaveBuildTarget, IHaveRestoreTarget, IHaveSolution, IHaveXamarinConfiguration, IHaveGitVersion, IOutputLogs
    {
        /// <summary>
        /// Gets the path for the info plist.
        /// </summary>
        public AbsolutePath InfoPlist { get; }

        /// <summary>
        /// Gets the path for the info plist.
        /// </summary>
        public string BaseBundleIdentifier => "com.rocketbooster.nuke";

        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
           .DependsOn(Restore)
           .Executes(
                () => MSBuild(
                    settings => settings
                       .SetSolutionFile(Solution)
                       .SetProperty("Platform", TargetPlatform.AnyCPU)
                       .SetConfiguration(Configuration)
                       .SetDefaultLoggers(LogsDirectory / "build.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetAssemblyVersion(GitVersion.AssemblySemVer)
                       .SetPackageVersion(GitVersion.NuGetVersionV2)
                )
            );

        /// <summary>
        /// modify info.plist
        /// </summary>
        public Target ModifyPlist => _ => _
           .Executes(() =>
{
    Logger.Trace($"Info.plist Path: {InfoPlist}");
    var plist = Plist.Deserialize(InfoPlist);
    var bundleIdentifier = !Equals(Configuration, XamarinConfiguration.Store)
        ? Configuration
        : string.Empty;

    plist["CFBundleIdentifier"] = $"{BaseBundleIdentifier}.{bundleIdentifier?.ToLower()}".TrimEnd('.');
    Logger.Info($"CFBundleIdentifier: {plist["CFBundleIdentifier"]}");

    plist["CFBundleShortVersionString"] = $"{GitVersion.Major}.{GitVersion.Minor}.{GitVersion.Patch}";
    Logger.Info($"CFBundleShortVersionString: {plist["CFBundleShortVersionString"]}");

    plist["CFBundleVersion"] = $"{GitVersion.PreReleaseNumber}";
    Logger.Info($"CFBundleVersion: {plist["CFBundleVersion"]}");

    Plist.Serialize(InfoPlist, plist);
}
            );
    }

    public interface IPackXamarinMac : IHavePackTarget, IHaveTestTarget, IHaveXamarinConfiguration, IOutputLogs, IHaveGitVersion, IHaveSolution
    {
        /// <summary>
        /// packages a binary for distribution.
        /// </summary>
        public Target Package => _ => _
           .DependsOn(Test)
           .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
           .Executes(
                () => MSBuild(
                    settings => settings
                       .SetSolutionFile(Solution)
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
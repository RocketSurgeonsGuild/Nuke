using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Xamarin iOS build
    /// </summary>
    public interface ICanBuildXamariniOs : IHaveBuildTarget,
                                           IHaveRestoreTarget,
                                           IHaveSolution,
                                           IHaveConfiguration,
                                           IHaveGitVersion,
                                           IHaveOutputLogs,
                                           IHaveiOSTargetPlatform
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
        public Target BuildiPhone => _ => _
           .DependsOn(Restore)
           .Executes(
                () => MSBuild(
                    settings => settings.SetSolutionFile(Solution)
                       .SetProperty("Platform", iOSTargetPlatform)
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
        public Target ModifyInfoPlist => _ => _
           .DependsOn(Restore)
           .Executes(
                () =>
                {
                    Logger.Trace($"Info.plist Path: {InfoPlist}");
                    var plist = Plist.Deserialize(InfoPlist);
                    var bundleIdentifier = !Equals(Configuration, XamarinConfiguration.Store.ToString())
                        ? Configuration
                        : string.Empty;

                    plist["CFBundleIdentifier"] = $"{BaseBundleIdentifier}.{bundleIdentifier?.ToLower()}".TrimEnd('.');
                    Logger.Info($"CFBundleIdentifier: {plist["CFBundleIdentifier"]}");

                    plist["CFBundleShortVersionString"] = $"{GitVersion.MajorMinorPatch}";
                    Logger.Info($"CFBundleShortVersionString: {plist["CFBundleShortVersionString"]}");

                    plist["CFBundleVersion"] = $"{GitVersion.PreReleaseNumber}";
                    Logger.Info($"CFBundleVersion: {plist["CFBundleVersion"]}");

                    Plist.Serialize(InfoPlist, plist);
                }
            );
    }
}
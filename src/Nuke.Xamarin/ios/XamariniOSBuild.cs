using System;
using System.Linq.Expressions;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin iOS based applications
    /// </summary>
    public class XamariniOSBuild : XamarinBuild
    {
        /// <summary>
        /// Gets the target platform.
        /// </summary>
        /// <value>The target platform.</value>
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public TargetPlatform TargetPlatform { get; }

        /// <summary>
        /// Gets the path for the info plist.
        /// </summary>
        public virtual AbsolutePath InfoPlist { get; }

        /// <summary>
        /// Gets the path for the info plist.
        /// </summary>
        public virtual string BaseBundleIdentifier { get; } = "com.rocketbooster.nuke";

        /// <summary>
        /// A value indicated whether the build host is OSX.
        /// </summary>
        public Expression<Func<bool>> IsOsx = () => EnvironmentInfo.Platform == PlatformFamily.OSX;

        /// <summary>
        /// Initializes a new instance of the <see cref="XamariniOSBuild"/> class.
        /// </summary>
        public XamariniOSBuild() => TargetPlatform = TargetPlatform.iPhone;

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
                });

        /// <summary>
        /// msbuild
        /// </summary>
        public static ITargetDefinition Build(ITargetDefinition _, IXamariniOSBuild build) => _
           .DependsOn(build.Restore)
           .Executes(
                () => MSBuild(
                    settings => settings
                       .SetSolutionFile(build.Solution)
                       .SetProperty("Platform", build.TargetPlatform)
                       .SetConfiguration(build.Configuration)
                       .SetDefaultLoggers(build.LogsDirectory / "build.log")
                       .SetGitVersionEnvironment(build.GitVersion)
                       .SetAssemblyVersion(build.GitVersion.AssemblySemVer)
                       .SetPackageVersion(build.GitVersion.NuGetVersionV2)));

        /// <summary>
        /// packages a binary for distribution.
        /// </summary>
        public static ITargetDefinition Package(ITargetDefinition _, IXamariniOSBuild build) => _
           .DependsOn(build.Test)
           .OnlyWhenStatic(build.IsOsx)
           .Executes(
                () => MSBuild(
                    settings => settings
                       .SetSolutionFile(build.Solution)
                       .SetProperty("Platform", build.TargetPlatform)
                       .SetProperty("BuildIpa", "true")
                       .SetProperty("ArchiveOnBuild", "true")
                       .SetConfiguration(build.Configuration)
                       .SetDefaultLoggers(build.LogsDirectory / "package.log")
                       .SetGitVersionEnvironment(build.GitVersion)
                       .SetAssemblyVersion(build.GitVersion.AssemblySemVer)
                       .SetPackageVersion(build.GitVersion.NuGetVersionV2)));
    }
}
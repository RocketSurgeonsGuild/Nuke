using Nuke.Common.IO;
using Nuke.Common.Tools.MSBuild;
using Serilog;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

#pragma warning disable CA1304
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Xamarin iOS build
/// </summary>
public interface ICanBuildXamariniOS : IHaveBuildTarget,
                                       IHaveRestoreTarget,
                                       IHaveSolution,
                                       IHaveConfiguration,
                                       IHaveGitVersion,
                                       IHaveOutputLogs,
                                       IHaveiOSTargetPlatform,
                                       ICan
{
    /// <summary>
    ///     Gets the path for the info plist.
    /// </summary>
    public AbsolutePath InfoPlist { get; }

    /// <summary>
    ///     Gets the path for the info plist.
    /// </summary>
    public string BaseBundleIdentifier => "com.rocketbooster.nuke";

    /// <summary>
    ///     msbuild
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
                                                                  .SetAssemblyVersion(GitVersion?.FullSemanticVersion())
                                                                  .SetPackageVersion(GitVersion?.NuGetVersionV2)
                                          )
                                      );


    /// <summary>
    ///     modify info.plist
    /// </summary>
    public Target ModifyInfoPlist => _ => _
                                         .DependsOn(Restore)
                                         .Executes(
                                              () =>
                                              {
                                                  Log.Verbose("Info.plist Path: {InfoPlist}", InfoPlist);
                                                  var plist = Plist.Deserialize(InfoPlist);
                                                  var bundleIdentifier = !Equals(Configuration, XamarinConfiguration.Store.ToString())
                                                      ? Configuration
                                                      : string.Empty;

                                                  plist["CFBundleIdentifier"] = $"{BaseBundleIdentifier}.{bundleIdentifier.ToLower()}".TrimEnd('.');
                                                  Log.Information("CFBundleIdentifier: {CFBundleIdentifier}", plist["CFBundleIdentifier"]);

                                                  plist["CFBundleShortVersionString"] = $"{GitVersion?.MajorMinorPatch}";
                                                  Log.Information(
                                                      "CFBundleShortVersionString: {CFBundleShortVersionString}", plist["CFBundleShortVersionString"]
                                                  );

                                                  plist["CFBundleVersion"] = $"{GitVersion?.FullSemVer.Replace('+', '.')}";
                                                  Log.Information("CFBundleVersion: {CFBundleVersion}", plist["CFBundleVersion"]);

                                                  Plist.Serialize(InfoPlist, plist);
                                              }
                                          );
}

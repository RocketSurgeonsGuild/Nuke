using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

#pragma warning disable CA1304
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Xamarin iOS build
/// </summary>
public interface ICanBuildXamariniOS : IXamarinAppleTarget,
                                       IHaveBuildTarget,
                                       IHaveRestoreTarget,
                                       IHaveSolution,
                                       IHaveConfiguration,
                                       IHaveIpa,
                                       IHaveOutputLogs,
                                       IHaveiOSTargetPlatform,
                                       ICan
{
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
                                                                  .SetAssemblyVersion(GitVersion.AssemblyVersion())
                                                                  .SetPackageVersion(GitVersion.PackageVersion())
                                          )
                                      );
}

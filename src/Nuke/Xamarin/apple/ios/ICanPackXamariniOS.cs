using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Xamarin iOS Pack
/// </summary>
public interface ICanPackXamariniOS : IHavePackTarget,
                                      IHaveConfiguration,
                                      IHaveTestTarget,
                                      IHaveOutputLogs,
                                      IHaveGitVersion,
                                      IHaveSolution,
                                      IHaveiOSTargetPlatform,
                                      ICan
{
    /// <summary>
    ///     packages a binary for distribution.
    /// </summary>
    public Target PackiPhone => _ => _.DependsOn(Test)
                                      .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
                                      .Executes(
                                           () =>
                                               MSBuild(
                                                   settings =>
                                                       settings.SetSolutionFile(Solution)
                                                               .SetProperty("Platform", iOSTargetPlatform)
                                                               .SetProperty("BuildIpa", "true")
                                                               .SetProperty("ArchiveOnBuild", "true")
                                                               .SetConfiguration(Configuration)
                                                               .SetDefaultLoggers(LogsDirectory / "package.log")
                                                               .SetGitVersionEnvironment(GitVersion)
                                                               .SetAssemblyVersion(GitVersion.AssemblyVersion())
                                                               .SetPackageVersion(GitVersion.PackageVersion())
                                               )
                                       );
}

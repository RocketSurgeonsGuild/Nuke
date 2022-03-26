using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Xamarin mac build
/// </summary>
public interface IHavePackXamarinMac : IHavePackTarget,
                                       IHaveTestTarget,
                                       IHaveConfiguration,
                                       IHaveOutputLogs,
                                       IHaveGitVersion,
                                       IHaveSolution,
                                       ICan
{
    /// <summary>
    ///     packages a binary for distribution.
    /// </summary>
    public Target Package => _ => _
                                 .DependsOn(Test)
                                 .OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
                                 .Executes(
                                      () => MSBuild(
                                          settings => settings.SetSolutionFile(Solution)
                                                              .SetProperty("Platform", TargetPlatform.AnyCPU)
                                                              .SetProperty("BuildIpa", "true")
                                                              .SetProperty("ArchiveOnBuild", "true")
                                                              .SetConfiguration(Configuration)
                                                              .SetDefaultLoggers(LogsDirectory / "package.log")
                                                              .SetGitVersionEnvironment(GitVersion)
                                                              .SetAssemblyVersion(GitVersion?.FullSemanticVersion())
                                                              .SetPackageVersion(GitVersion?.NuGetVersionV2)
                                      )
                                  );
}

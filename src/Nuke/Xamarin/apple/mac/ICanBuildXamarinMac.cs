using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

#pragma warning disable CA1304
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Xamarin mac build
/// </summary>
public interface ICanBuildXamarinMac : IXamarinAppleTarget,
    IHaveBuildTarget,
    IHaveRestoreTarget,
    IHaveSolution,
    IHaveConfiguration,
    IHaveOutputLogs,
    ICan
{
    /// <summary>
    ///     msbuild
    /// </summary>
    new Target Build => d => d
                                   .DependsOn(Restore)
                                   .Executes(
                                        () => MSBuild(
                                            settings => settings
                                                       .SetSolutionFile(Solution)
                                                       .SetProperty("Platform", TargetPlatform.AnyCPU)
                                                       .SetConfiguration(Configuration)
                                                       .SetDefaultLoggers(LogsDirectory / "build.log")
                                                       .SetGitVersionEnvironment(GitVersion)
                                                       .SetAssemblyVersion(GitVersion.AssemblyVersion())
                                                       .SetPackageVersion(GitVersion.PackageVersion())
                                        )
                                    );
}

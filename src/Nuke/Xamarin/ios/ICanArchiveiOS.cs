using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

#pragma warning disable CA1304
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    ///     archives a binary.
    /// </summary>
    public interface ICanArchiveiOS : IHavePackTarget,
                                      IHaveConfiguration,
                                      IHaveOutputLogs,
                                      IHaveGitVersion,
                                      IHaveSolution,
                                      IHaveiOSTargetPlatform,
                                      IHaveEnableRestore,
                                      ICan
    {
        /// <summary>
        ///     packages a binary for distribution.
        /// </summary>
        public Target ArchiveIpa => _ => _.OnlyWhenStatic(() => EnvironmentInfo.Platform == PlatformFamily.OSX)
                                          .Executes(
                                               () =>
                                                   MSBuild(
                                                       settings =>
                                                           settings.SetSolutionFile(Solution)
                                                                   .SetRestore(EnableRestore)
                                                                   .SetProperty("Platform", iOSTargetPlatform)
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
}

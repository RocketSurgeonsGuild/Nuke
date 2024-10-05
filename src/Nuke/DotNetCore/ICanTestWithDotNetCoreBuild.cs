using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet test` test run with code coverage via coverlet
/// </summary>
[PublicAPI]
public interface ICanTestWithDotNetCoreBuild : IHaveCollectCoverage,
    IHaveBuildTarget,
    ITriggerCodeCoverageReports,
    IComprehendTests,
    IHaveTestArtifacts,
    IHaveGitVersion,
    IHaveSolution,
    IHaveConfiguration,
    IHaveOutputLogs,
    ICan
{
    /// <summary>
    ///     dotnet test
    /// </summary>
    [NonEntryTarget]
    public Target DotnetCoreTestBuild => d => d
                                             .Description("Executes all the unit tests.")
                                             .Unlisted()
                                             .After(Build)
                                             .TryDependentFor<IHaveTestTarget>(a => a.Test)
                                             .TryAfter<IHaveRestoreTarget>(a => a.Restore)
                                             .WhenSkipped(DependencyBehavior.Execute)
                                             .Executes(
                                                  () => MSBuildTasks.MSBuild(
                                                      settings =>
                                                          settings
                                                             .SetProcessWorkingDirectory(RootDirectory)
                                                             .SetSolutionFile(Solution)
                                                             .SetConfiguration(TestBuildConfiguration)
                                                             .SetDefaultLoggers(LogsDirectory / "test.build.log")
                                                             .SetGitVersionEnvironment(GitVersion)
                                                             .SetAssemblyVersion(GitVersion.AssemblySemVer)
                                                             .SetPackageVersion(GitVersion.NuGetVersionV2)
                                                  )
                                              )
                                             .CreateOrCleanDirectory(TestResultsDirectory)
                                             .CleanCoverageDirectory(CoverageDirectory)
                                             .EnsureRunSettingsExists(RunSettings)
                                             .Executes(
                                                  () => DotNetTasks.DotNetTest(
                                                      s => s
                                                          .SetProcessWorkingDirectory(RootDirectory)
                                                          .SetProjectFile(Solution)
                                                          .SetDefaultLoggers(LogsDirectory / "test.log")
                                                          .SetGitVersionEnvironment(GitVersion)
                                                          .EnableNoRestore()
                                                          .SetLoggers("trx")
                                                          .SetConfiguration(TestBuildConfiguration)
                                                          .EnableNoBuild()
                                                          .SetResultsDirectory(TestResultsDirectory)
                                                          .When(
                                                               !CollectCoverage,
                                                               x => x
                                                                   .SetProperty((string)"CollectCoverage", "true")
                                                                   .SetProperty("CoverageDirectory", CoverageDirectory)
                                                           )
                                                          .When(
                                                               CollectCoverage,
                                                               x => x
                                                                   .SetProperty("CollectCoverage", "false")
                                                                   .SetDataCollector("XPlat Code Coverage")
                                                                   .SetSettingsFile(RunSettings)
                                                           )
                                                  )
                                              )
                                             .CollectCoverage(TestResultsDirectory, CoverageDirectory);
}

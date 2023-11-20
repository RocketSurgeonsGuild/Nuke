using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet test` test run with code coverage via coverlet
/// </summary>
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
    public Target CoreTest => d => d
                                  .Description("Executes all the unit tests.")
                                  .Unlisted()
                                  .After(Build)
                                  .OnlyWhenDynamic(() => TestsDirectory.GlobFiles("**/*.csproj").Count > 0)
                                  .WhenSkipped(DependencyBehavior.Execute)
                                  .Executes(
                                       () => MSBuildTasks.MSBuild(
                                           settings =>
                                               settings
                                                  .SetSolutionFile(Solution)
                                                  .SetConfiguration(Configuration)
                                                  .SetDefaultLoggers(LogsDirectory / "test.build.log")
                                                  .SetGitVersionEnvironment(GitVersion)
                                                  .SetAssemblyVersion(GitVersion?.AssemblySemVer)
                                                  .SetPackageVersion(GitVersion?.NuGetVersionV2)
                                       )
                                   )
                                  .CreateOrCleanDirectory(TestResultsDirectory)
                                  .CleanCoverageDirectory(CoverageDirectory)
                                  .EnsureRunSettingsExists(RunSettings)
                                  .Executes(
                                       () => DotNetTasks.DotNetTest(
                                           s => s.SetProjectFile(Solution)
                                                 .SetDefaultLoggers(LogsDirectory / "test.log")
                                                 .SetGitVersionEnvironment(GitVersion)
                                                 .EnableNoRestore()
                                                 .SetLoggers("trx")
                                                 .SetConfiguration(Configuration)
                                                 .EnableNoBuild()
                                                 // DeterministicSourcePaths being true breaks coverlet!
                                                 .SetProperty("DeterministicSourcePaths", "false")
                                                 .SetResultsDirectory(TestResultsDirectory)
                                                 .When(
                                                      !CollectCoverage,
                                                      x => x.SetProperty((string)"CollectCoverage", "true")
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

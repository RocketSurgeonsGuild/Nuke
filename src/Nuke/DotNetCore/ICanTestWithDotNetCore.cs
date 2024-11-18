using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet test` test run with code coverage via coverlet
/// </summary>
[PublicAPI]
public interface ICanTestWithDotNetCore : IHaveBuildTarget,
    ITriggerCodeCoverageReports,
    IComprehendTests,
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
    public Target DotnetCoreTest => d => d
                                        .Description("Executes all the unit tests.")
                                        .Unlisted()
                                        .After(Build)
                                        .TryDependentFor<IHaveTestTarget>(a => a.Test)
                                        .TryAfter<IHaveRestoreTarget>(a => a.Restore)
                                        .WhenSkipped(DependencyBehavior.Execute)
                                        .Net9MsBuildFix()
                                        .Executes(
                                             () => DotNetTasks.DotNetBuild(
                                                 s => s
                                                     .SetProcessWorkingDirectory(RootDirectory)
                                                     .SetProjectFile(Solution)
                                                     .SetDefaultLoggers(LogsDirectory / "test.build.log")
                                                     .SetGitVersionEnvironment(GitVersion)
                                                     .SetConfiguration(TestBuildConfiguration)
                                                     .EnableNoRestore()
                                             )
                                         )
                                        .CreateOrCleanDirectory(TestResultsDirectory)
                                        .EnsureRunSettingsExists(RunSettings)
                                        .Net9MsBuildFix()
                                        .Executes(
                                             () => DotNetTasks.DotNetTest(
                                                 s => CustomizeDotNetTestSettings(
                                                     s
                                                        .SetProcessWorkingDirectory(RootDirectory)
                                                        .SetProjectFile(Solution)
                                                        .SetDefaultLoggers(LogsDirectory / "test.log")
                                                        .SetGitVersionEnvironment(GitVersion)
                                                        .SetConfiguration(TestBuildConfiguration)
                                                        .EnableNoRestore()
                                                        .EnableNoBuild()
                                                        .SetLoggers("trx")
                                                        .SetResultsDirectory(TestResultsDirectory)
                                                        .SetSettingsFile(RunSettings)
                                                        .SetDataCollector(DataCollector)
                                                 )
                                             )
                                         );

    public DotNetTestSettings CustomizeDotNetTestSettings(DotNetTestSettings settings)
    {
        return settings;
    }
}

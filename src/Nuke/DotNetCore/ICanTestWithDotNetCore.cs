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
                                        .EnsureRunSettingsExists(this)
                                        .Net9MsBuildFix()
                                        .Executes(
                                             () => DotNetTool.GetTool("dotnet-coverage")(
                                                 $"{new Arguments()
                                                   .Add("collect")
                                                   .Add("--settings {value}", RunSettings)
                                                   .Add("--output {value}", TestResultsDirectory / "test.coverage")
                                                   .Add("--output-format {value}", "coverage")
                                                   .Add("--")
                                                   .Add("dotnet")
                                                   .Concatenate(
                                                        CustomizeDotNetTestSettings(
                                                                new DotNetTestSettings()
                                                                   .SetProcessWorkingDirectory(RootDirectory)
                                                                   .SetProjectFile(Solution)
                                                                   .SetDefaultLoggers(LogsDirectory / "test.log")
                                                                   .SetGitVersionEnvironment(GitVersion)
                                                                   .SetConfiguration(TestBuildConfiguration)
                                                                   .EnableNoRestore()
                                                                   .EnableNoBuild()
                                                                   .SetLoggers("trx")
                                                                   .SetResultsDirectory(TestResultsDirectory)
                                                            )
                                                           .RemoveLoggers()
                                                           .GetProcessArguments()
                                                    )
                                                   .RenderForExecution()}",
                                                 RootDirectory
                                             )
                                         );

    public DotNetTestSettings CustomizeDotNetTestSettings(DotNetTestSettings settings) => settings;
}

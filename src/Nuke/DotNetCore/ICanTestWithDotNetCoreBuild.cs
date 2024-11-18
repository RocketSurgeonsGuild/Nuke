using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet test` test run with code coverage via coverlet
/// </summary>
[PublicAPI]
public interface ICanTestWithDotNetCoreBuild : IHaveBuildTarget,
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
                                             .EnsureRunSettingsExists(this)
                                             .Net9MsBuildFix()
                                             .Executes(
                                                  () => DotNetTool.GetTool("dotnet-coverage")(
                                                      $"{new Arguments()
                                                        .Add("collect")
                                                        .Add("--settings {value}", RunSettings)
                                                        .Add("--output {value}", CoverageDirectory / "coverage.cobertura.xml")
                                                        .Add("--output-format {value}", "cobertura")
                                                        .Add("--")
                                                        .Add("dotnet")
                                                        .Concatenate(
                                                             (Arguments)CustomizeDotNetTestSettings(
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
                                                                .GetProcessArguments()
                                                         ).RenderForExecution()}",
                                                      RootDirectory
                                                  )
                                              );

    public DotNetTestSettings CustomizeDotNetTestSettings(DotNetTestSettings settings)
    {
        return settings;
    }
}

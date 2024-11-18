using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReSharper;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet test` test run with code coverage via coverlet
/// </summary>
[PublicAPI]
public interface ICanTestWithDotNetCore : IHaveBuildTarget,
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
                                             () =>
                                             {
                                                 var targetSettings = new DotNetTestSettings()
                                                                     .SetProcessWorkingDirectory(RootDirectory)
                                                                     .SetProjectFile(Solution)
                                                                     .SetDefaultLoggers(LogsDirectory / "test.log")
                                                                     .SetGitVersionEnvironment(GitVersion)
                                                                     .SetConfiguration(TestBuildConfiguration)
                                                                     .EnableNoRestore()
                                                                     .EnableNoBuild()
                                                                     .SetResultsDirectory(TestResultsDirectory);

                                                 DotCoverTasks.DotCoverCoverDotNet(
                                                     settings => settings
                                                                .SetFilters(
                                                                     "-:Bogus*",
                                                                     "-:FakeItEasy*",
                                                                     "-:Moq*",
                                                                     "-:NSubstitute*",
                                                                     "-:Verify*",
                                                                     "-:XUnit*",
                                                                     "-:TUnit*",
                                                                     "-:Microsoft*",
                                                                     "-:System*",
                                                                     "-:JetBrains*",
                                                                     "-:DryIoc*",
                                                                     "-:Nuke*",
                                                                     "-:testhost*",
                                                                     "-:FluentAssertions*",
                                                                     "-:Serilog*",
                                                                     "-:module=JetBrains*",
                                                                     "-:class=JetBrains*"
                                                                 )
                                                                .AddAttributeFilters(
                                                                     "System.Diagnostics.DebuggerHiddenAttribute",
                                                                     "System.Diagnostics.DebuggerNonUserCodeAttribute",
                                                                     "System.CodeDom.Compiler.GeneratedCodeAttribute",
                                                                     "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
                                                                     "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
                                                                 )
                                                                .SetProcessWorkingDirectory(RootDirectory)
                                                                .SetTargetWorkingDirectory(RootDirectory)
                                                                .SetReportType(DotCoverReportType.DetailedXml)
                                                                .SetOutputFile(TestResultsDirectory / "test.dotcover.xml")
                                                                .SetTargetArguments(targetSettings.GetProcessArguments().RenderForExecution())
                                                 );
                                             }
                                         );
}

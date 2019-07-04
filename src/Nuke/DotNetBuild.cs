using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using Nuke.Common.Tools.VSTest;
using static Nuke.Common.Tools.VSTest.VSTestTasks;
using Nuke.Common.Tools.VSWhere;
using System.Linq;
using System.Xml.Linq;
using System;

namespace Rocket.Surgery.Nuke
{
    // public abstract class DotNetBuild : RocketBuild
    // {
    //     public Target Core => _ => _;

    //     public Target InstallTools => _ => _
    //         .DependentFor(Core)
    //         .Unlisted()
    //         .Executes(() => DotNet("tool restore"));

    //     public Target Restore => _ => _
    //         .DependentFor(Core)
    //         .DependsOn(InstallTools)
    //         .Executes(() =>
    //         {
    //             MSBuild(s => s
    //                 .SetTargets("Restore")
    //                 .SetProjectFile(Solution)
    //                 .DisableRestoreDisableParallel()
    //                 .SetBinaryLogger(LogsDirectory / "restore.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
    //                 .SetFileLogger(LogsDirectory / "restore.log", Verbosity)
    //                 .SetGitVersionEnvironment(GitVersion)
    //             );
    //         });

    //     public Target Build => _ => _
    //         .DependsOn(Restore)
    //         .DependentFor(Core)
    //         .Executes(() =>
    //         {
    //             MSBuild(s => s
    //                 .SetTargets("Build")
    //                 .SetProjectFile(Solution)
    //                 .SetBinaryLogger(LogsDirectory / "build.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
    //                 .SetFileLogger(LogsDirectory / "build.log", Verbosity)
    //                 .SetGitVersionEnvironment(GitVersion)
    //                 .SetConfiguration(Configuration)
    //                 .DisableRestore());
    //         });

    //     public Target Test => _ => _
    //         .DependsOn(Build)
    //         .DependentFor(Core)
    //         .DependentFor(Pack)
    //         .Triggers(Generate_Code_Coverage_Reports)
    //         .OnlyWhenStatic(() => TestDirectory.GlobFiles("**/*.csproj").Count > 0)
    //         .WhenSkipped(DependencyBehavior.Execute)
    //         .Executes(() =>
    //         {
    //         // TestDirectory.GlobFiles("**/*.csproj")
    //         //     .ForEach((Project) =>
    //         //     {
    //         //         var name = Path.GetFileNameWithoutExtension(Project).ToLowerInvariant();
    //         //         // var name = Project.
    //         //         DotNetTest(s => s
    //         //             .SetProjectFile(Project)
    //         //             .SetBinaryLogger(LogsDirectory / $"{name}.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
    //         //             .SetFileLogger(LogsDirectory / $"{name}.log", Verbosity)
    //         //             .SetGitVersionEnvironment(GitVersion)
    //         //             .SetConfiguration(Configuration)
    //         //             .EnableNoRestore()
    //         //             .SetLogger($"trx;LogFileName={TestResultsDirectory / $"{name}.trx"}")
    //         //             .SetProperty("CollectCoverage", true)
    //         //             .SetProperty("CoverageDirectory", CoverageDirectory)
    //         //             .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
    //         //     });

    //         var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), Array.Empty<object>());
    //             var runSettings = new XElement("RunSettings");
    //             var runConfiguration = new XElement("RunConfiguration");
    //             var resultsDirectory = new XElement("ResultsDirectory");
    //             resultsDirectory.SetValue(TestResultsDirectory);
    //             document.Add(runSettings);
    //             runSettings.Add(runConfiguration);
    //             runConfiguration.Add(resultsDirectory);
    //             document.Save(TemporaryDirectory / ".runsettings");

    //             VSTest(s => s
    //                 .SetTests(TestDirectory.GlobFiles($"*/bin/{Configuration}/**/*.Tests.dll").Select(x => x.ToString()))
    //                 .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
    //                 .SetFileLogger(LogsDirectory / "test.log", Verbosity)
    //                 .SetGitVersionEnvironment(GitVersion)
    //                 .SetSettingsFile(TemporaryDirectory / ".runsettings")
    //             // TODO: This needs to be done
    //             // .SetLogger($"trx")
    //             // .SetProperty("CollectCoverage", true)
    //             // .SetProperty("CoverageDirectory", CoverageDirectory)
    //             // .SetProperty("VSTestResultsDirectory", TestResultsDirectory)
    //             );
    //         });

    //     public Target Pack => _ => _
    //         .DependsOn(Build)
    //         .DependentFor(Core)
    //         .Executes(() =>
    //         {
    //             DotNetPack(s => s
    //                 .SetProject(Solution)
    //                 .SetVersion(GitVersion.FullSemVer)
    //                 .SetIncludeSource(IncludeSource)
    //                 .SetIncludeSymbols(IncludeSymbols)
    //                 .SetBinaryLogger(LogsDirectory / "pack.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
    //                 .SetFileLogger(LogsDirectory / "pack.log", Verbosity)
    //                 .SetGitVersionEnvironment(GitVersion)
    //                 .SetConfiguration(Configuration)
    //                 .EnableNoRestore()
    //                 .SetOutputDirectory(NuGetPackageDirectory));
    //         });
    // }
}

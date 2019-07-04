using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Tools.ReportGenerator;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using Nuke.Common.Tools.VSWhere;
using static Nuke.Common.Tools.VSWhere.VSWhereTasks;
using System.IO;
using System.Linq;

namespace Rocket.Surgery.Nuke
{
    public abstract class RocketBoosterBuild : NukeBuild
    {
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Solution] public readonly Solution Solution;
        [GitRepository] public readonly GitRepository GitRepository;
        [ComputedGitVersion] public readonly GitVersion GitVersion;

        public AbsolutePath SourceDirectory => RootDirectory / "src";
        public AbsolutePath TestDirectory => RootDirectory / "test";
        public AbsolutePath ArtifactsDirectory => Variable("Artifacts") != null ? (AbsolutePath)Variable("Artifacts") : RootDirectory / "artifacts";
        public AbsolutePath LogsDirectory => ArtifactsDirectory / "logs";
        public AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test";
        public AbsolutePath NuGetPackageDirectory => ArtifactsDirectory / "nuget";
        public AbsolutePath CoverageDirectory => Variable("Coverage") != null ? (AbsolutePath)Variable("Coverage") : RootDirectory / "coverage";


        /// <summary>
        /// Gets or sets a value indicating whether [include symbols].
        /// </summary>
        /// <value><c>true</c> if [include symbols]; otherwise, <c>false</c>.</value>
        public bool IncludeSymbols { get; set; } = true;
        /// <summary>
        /// Gets or sets a value indicating whether [include source].
        /// </summary>
        /// <value><c>true</c> if [include source]; otherwise, <c>false</c>.</value>
        public bool IncludeSource { get; set; } = true;

        public Target Clean => _ => _
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(ArtifactsDirectory);
                EnsureCleanDirectory(CoverageDirectory);
            });

        public Target Generate_Code_Coverage_Reports => _ => _
            .Description("Generates code coverage reports")
            .Unlisted()
            .OnlyWhenDynamic(() => CoverageDirectory.GlobFiles("**/*.cobertura.xml").Count > 0)
            .Executes(() =>
            {
                var reports = CoverageDirectory.GlobFiles("**/*.cobertura.xml").Select(z => z.ToString());
                ReportGenerator(s => s
                        .SetReports(reports)
                        .SetTargetDirectory(CoverageDirectory / "report")
                        .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTag(GitVersion.InformationalVersion)
                    );

                ReportGenerator(s => s
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory)
                    .SetReportTypes(ReportTypes.Cobertura)
                    .SetTag(GitVersion.InformationalVersion)
                );

                ReportGenerator(s => s
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory / "badges")
                    .SetReportTypes(ReportTypes.Badges)
                    .SetTag(GitVersion.InformationalVersion)
                );

                ReportGenerator(s => s
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory / "summary")
                    .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                    .SetTag(GitVersion.InformationalVersion)
                );
                RenameFile(CoverageDirectory / "Cobertura.xml", "solution.xml");
            });
    }

    public abstract class DotNetCoreBuild : RocketBoosterBuild
    {
        public Target Core => _ => _;

        public Target InstallTools => _ => _
            .DependsOn(Clean)
            .DependentFor(Core)
            .Unlisted()
            .Executes(() => DotNet("tool restore"));

        public Target Restore => _ => _
            .DependentFor(Core)
            .DependsOn(InstallTools)
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution)
                    .SetDisableParallel(true)
                    .SetBinaryLogger(LogsDirectory / "restore.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "restore.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                );
            });

        public Target Build => _ => _
            .DependsOn(Restore)
            .DependentFor(Core)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetBinaryLogger(LogsDirectory / "build.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "build.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            });

        public Target Test => _ => _
            .DependsOn(Build)
            .DependentFor(Core)
            .DependentFor(Pack)
            .Triggers(Generate_Code_Coverage_Reports)
            .OnlyWhenStatic(() => TestDirectory.GlobFiles("**/*.csproj").Count > 0)
            .WhenSkipped(DependencyBehavior.Execute)
            .Executes(() =>
            {
            // TestDirectory.GlobFiles("**/*.csproj")
            //     .ForEach((Project) =>
            //     {
            //         var name = Path.GetFileNameWithoutExtension(Project).ToLowerInvariant();
            //         // var name = Project.
            //         DotNetTest(s => s
            //             .SetProjectFile(Project)
            //             .SetBinaryLogger(LogsDirectory / $"{name}.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
            //             .SetFileLogger(LogsDirectory / $"{name}.log", Verbosity)
            //             .SetGitVersionEnvironment(GitVersion)
            //             .SetConfiguration(Configuration)
            //             .EnableNoRestore()
            //             .SetLogger($"trx;LogFileName={TestResultsDirectory / $"{name}.trx"}")
            //             .SetProperty("CollectCoverage", true)
            //             .SetProperty("CoverageDirectory", CoverageDirectory)
            //             .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
            //     });

            DotNetTest(s => s
                    .SetProjectFile(Solution)
                    .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "test.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetLogger($"trx")
                    .SetProperty("CollectCoverage", true)
                    .SetProperty("CoverageDirectory", CoverageDirectory)
                    .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
            });

        public Target Pack => _ => _
            .DependsOn(Build)
            .DependentFor(Core)
            .Executes(() =>
            {
                DotNetPack(s => s
                    .SetProject(Solution)
                    .SetVersion(GitVersion.FullSemVer)
                    .SetIncludeSource(IncludeSource)
                    .SetIncludeSymbols(IncludeSymbols)
                    .SetBinaryLogger(LogsDirectory / "pack.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "pack.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetOutputDirectory(NuGetPackageDirectory));
            });
    }
}

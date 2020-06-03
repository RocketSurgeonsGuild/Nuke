using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.Readme;
using Temp.CleanupCode;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Execution.InjectionUtility;
using static Temp.CleanupCode.CleanupCodeTasks;
using Nuke.Common.Tools.ReportGenerator;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

namespace Rocket.Surgery.Nuke
{
    public interface IHaveTestTarget
    {
        public Target Test { get; }
    }
    
    public interface IHaveBuildTarget
    {
        public Target Build { get; }
    }
    
    public interface IHavePackTarget
    {
        public Target Pack { get; }
    }
    
    public interface IHaveRestoreTarget
    {
        public Target Restore { get; }
    }
    
    public interface IHaveConfiguration
    {
        string Configuration { get; }
    }
    public interface ICanLintMyself : IHaveSolution
    {
        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The files to lint, if not given lints all files", Separator = " ")]
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] LintFiles => GetInjectionValue(() => LintFiles ?? Array.Empty<string>());
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The profile to use for linting")]
        public string LintProfile => GetInjectionValue(() => LintProfile ?? "Full Cleanup");

        /// <summary>
        /// Applies code cleanup tasks
        /// </summary>
        public Target Lint => _ => _
           .Requires(() => LintFiles)
           .Executes(
                () =>
                {
                    CleanupCode(
                        x => x
                           .SetTargetPath(Solution.Path)
                           .SetProfile(LintProfile)
                           .AddInclude(LintFiles)
                    );
                }
            );
    }

    public interface IHaveGitRepository
    {
        /// <summary>
        /// The Git Repository currently being built
        /// </summary>
        [GitRepository]
        public GitRepository? GitRepository => GetInjectionValue(() => GitRepository);
    }

    public interface IHaveGitVersion : IHaveGitRepository
    {
        GitVersion GitVersion { get; }
    }

    public interface IReadmeUpdater : IHaveSolution, IMayTheForceBeWithYou
    {
        /// <summary>
        /// The readme updater that ensures that all the badges are in sync.
        /// </summary>
        [Readme]
        public ReadmeUpdater Readme => GetInjectionValue(() => Readme);

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while
        /// keeping the rest of the readme correct.
        /// </summary>
        public Target GenerateReadme => _ => _
           .Unlisted()
           .OnlyWhenStatic(() => NukeBuild.IsLocalBuild)
           .Executes(
                () =>
                {
                    var readmeContent = File.ReadAllText(NukeBuild.RootDirectory / "Readme.md");
                    readmeContent = Readme.Process(readmeContent, this);
                    File.WriteAllText(NukeBuild.RootDirectory / "Readme.md", readmeContent);
                }
            );
    }

    public interface IHaveArtifacts
    {
        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
        public AbsolutePath ArtifactsDirectory => GetVariable<AbsolutePath>("Artifacts")
         ?? GetInjectionValue(() => ArtifactsDirectory)
         ?? NukeBuild.RootDirectory / "artifacts";
    }

    public interface IOutputArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where packaged output should be placed (zip, webdeploy, etc)
        /// </summary>
        public AbsolutePath OutputDirectory => ArtifactsDirectory / "output";
    }

    public interface IPublishArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where publish output should be placed
        /// </summary>
        public AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";
    }

    public interface IOutputLogs : IHaveArtifacts
    {
        /// <summary>
        /// The directory where logs will be placed
        /// </summary>
        public AbsolutePath LogsDirectory => ArtifactsDirectory / "logs";
    }

    public interface IOutputTestArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where test results will be placed
        /// </summary>
        public AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test";
    }

    public interface IOutputNuGetArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where nuget packages will be placed
        /// </summary>
        public AbsolutePath NuGetPackageDirectory => ArtifactsDirectory / "nuget";
    }

    public interface IIncludeCodeCoverage : IHaveArtifacts
    {
        /// <summary>
        /// The directory where coverage artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
        public AbsolutePath CoverageDirectory => GetVariable<AbsolutePath>("Coverage")
         ?? GetInjectionValue(() => CoverageDirectory)
         ?? NukeBuild.RootDirectory / "coverage";
    }

    public interface IIncludeSamples
    {
        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SampleDirectory => NukeBuild.RootDirectory / "sample";
    }

    public interface IIncludeSources
    {
        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SourceDirectory => NukeBuild.RootDirectory / "src";
    }

    public interface IIncludeTemplates
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public AbsolutePath TemplatesDirectory => NukeBuild.RootDirectory / "templates";
    }

    public interface IIncludeTests
    {
        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestDirectory => DirectoryExists(NukeBuild.RootDirectory / "tests")
            ? NukeBuild.RootDirectory / "tests"
            : NukeBuild.RootDirectory / "test";

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestsDirectory => TestDirectory;
    }

    public interface IMayTheForceBeWithYou
    {
        /// <summary>
        /// Force a clean build, otherwise leave some incremental build pieces
        /// </summary>
        [Parameter("Force a clean build")]
        public bool Force { get; }
    }

    public interface IHaveBuildVersion : IHaveGitVersion, IHaveSolution
    {
        /// <summary>
        /// prints the build information.
        /// </summary>
        public Target BuildVersion => _ => _
           .Executes(
                () =>
                {
                    Logger.Info(
                        "Building version {0} of {1} using version {2} of Nuke.",
                        GitVersion?.NuGetVersionV2 ?? GitVersion?.NuGetVersion,
                        Solution.Name,
                        typeof(NukeBuild).Assembly.GetVersionText()
                    );
                }
            );
    }

    public interface IHaveSolution
    {
        /// <summary>
        /// The solution currently being build
        /// </summary>
        [Solution]
        public Solution Solution => GetInjectionValue(() => Solution);
    }

    public interface IHaveCleanTarget
    {
        Target Clean { get; }
    }

    public interface ICanClean : IHaveCleanTarget, IHaveBuildTarget
    {
        /// <summary>
        /// clean all artifact directories
        /// </summary>
        public Target Clean => _ => _
           .Before(Build)
           .Executes(
                () =>
                {
                    if (this is IHaveArtifacts artifacts)
                    {
                        EnsureCleanDirectory(artifacts.ArtifactsDirectory);
                        if (artifacts is IOutputLogs logs)
                            EnsureExistingDirectory(logs.LogsDirectory);
                        if (artifacts is IOutputTestArtifacts testArtifacts)
                            EnsureExistingDirectory(testArtifacts.TestResultsDirectory);
                        if (artifacts is IOutputNuGetArtifacts nuGetArtifacts)
                            EnsureExistingDirectory(nuGetArtifacts.NuGetPackageDirectory);
                        if (artifacts is IPublishArtifacts publishArtifacts)
                            EnsureExistingDirectory(publishArtifacts.PublishDirectory);
                        if (artifacts is IOutputArtifacts outputArtifacts)
                            EnsureExistingDirectory(outputArtifacts.OutputDirectory);
                    }

                    if (this is IIncludeCodeCoverage codeCoverage)
                    {
                        EnsureCleanDirectory(codeCoverage.CoverageDirectory);
                    }

                    if (this is IMayTheForceBeWithYou forceBeWithYou && forceBeWithYou.Force)
                    {
                        if (this is IIncludeSamples samples && DirectoryExists(samples.SampleDirectory))
                        {
                            samples.SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IIncludeSources sources && DirectoryExists(sources.SourceDirectory))
                        {
                            sources.SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IIncludeTemplates templates && DirectoryExists(templates.TemplatesDirectory))
                        {
                            templates.TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IIncludeTests tests && DirectoryExists(tests.TestDirectory))
                        {
                            tests.TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }
                    }
                }
            );
    }

    public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports
    {
        public Target Generate_Code_Coverage_Report => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .Executes(
                () => ReportGenerator(
                    s => WithTag(s)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageReportDirectory)
                       .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                )
            );
    }

    public interface IGenerateCodeCoverageSummary : ITriggerCodeCoverageReports
    {
        public Target Generate_Code_Coverage_Summary => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .Executes(
                () => ReportGenerator(
                        s => WithTag(s)
                            // .SetToolPath(toolPath)
                           .SetReports(InputReports)
                           .SetTargetDirectory(CoverageDirectory / "summary")
                           .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                    )
            );
    }

    public interface IGenerateCodeCoverageBadges : ITriggerCodeCoverageReports
    {
        public Target Generate_Code_Coverage_Badges => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .Executes(
                () => ReportGenerator(
                    s => WithTag(s)
                        // .SetToolPath(toolPath)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageDirectory / "badges")
                       .SetReportTypes(ReportTypes.Badges)
                )
            );
    }

    public interface ITriggerCodeCoverageReports : IIncludeCodeCoverage, IHaveTestTarget
    {
        public AbsolutePath CoverageReportDirectory => CoverageDirectory / "report";

        public IEnumerable<string> InputReports => CoverageDirectory
           .GlobFiles("**/*.cobertura.xml")
           .Select(z => z.ToString());

        protected ReportGeneratorSettings WithTag(ReportGeneratorSettings settings)
        {
            settings = settings.SetToolPath(
                ToolPathResolver.GetPackageExecutable(
                    "ReportGenerator",
                    "ReportGenerator.dll",
                    framework: "netcoreapp3.0"
                )
            );
            if (this is IHaveGitVersion gitVersion)
            {
                return settings.SetTag(gitVersion.GitVersion.InformationalVersion);
            }

            if (this is IHaveGitRepository gitRepository)
            {
                return settings.SetTag(gitRepository.GitRepository.Head);
            }

            return settings;
        }

        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Trigger_Code_Coverage_Reports => _ => _
           .TriggeredBy(Test)
           .After(Test)
           .Description("Generates code coverage reports")
           .Unlisted()
           .OnlyWhenDynamic(() => CoverageDirectory.GlobFiles("**/*.cobertura.xml").Count > 0);

        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Generate_Code_Coverage_Report_Cobertura => _ => _
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .OnlyWhenDynamic(() => CoverageDirectory.GlobFiles("**/*.cobertura.xml").Count > 0)
           .Executes(
                () =>
                {
                    // var toolPath = ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
                    ReportGenerator(
                        s => WithTag(s)
                            // .SetToolPath(toolPath)
                           .SetReports(InputReports)
                           .SetTargetDirectory(CoverageDirectory)
                           .SetReportTypes(ReportTypes.Cobertura)
                    );

                    CopyFile(
                        CoverageDirectory / "Cobertura.xml",
                        CoverageDirectory / "solution.cobertura",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                    CopyFile(
                        CoverageDirectory / "Cobertura.xml",
                        CoverageDirectory / "solution.xml",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                    RenameFile(
                        CoverageDirectory / "solution.xml",
                        CoverageDirectory / "cobertura.xml",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                }
            );
    }
}
using System;
using System.Collections.Concurrent;
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
    /// <summary>
    /// Defines the test target
    /// </summary>
    public interface IHaveTestTarget
    {
        /// <summary>
        /// The Test Target
        /// </summary>
        public Target Test { get; }
    }

    /// <summary>
    /// Defines the build target
    /// </summary>
    public interface IHaveBuildTarget
    {
        /// <summary>
        /// The Build Target
        /// </summary>
        public Target Build { get; }
    }

    /// <summary>
    /// Defines the pack target
    /// </summary>
    public interface IHavePackTarget
    {
        /// <summary>
        /// The Pack Target
        /// </summary>
        public Target Pack { get; }
    }

    /// <summary>
    /// Defines the restore target
    /// </summary>
    public interface IHaveRestoreTarget
    {
        /// <summary>
        /// The Restore Target
        /// </summary>
        public Target Restore { get; }
    }

    /// <summary>
    /// Defines a common property for build configuration
    /// </summary>
    public interface IHaveConfiguration
    {
        /// <summary>
        /// The build configuration
        /// </summary>
        string Configuration { get; }
    }

    /// <summary>
    /// Defines the configuration as strongly typed enumeration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHaveConfiguration<T> : IHaveConfiguration
        where T : Enumeration
    {
        /// <summary>
        /// The build configuration
        /// </summary>
        public new T Configuration { get; }

        string IHaveConfiguration.Configuration => Configuration.ToString();
    }

    /// <summary>
    /// Adds support for linting the files in a solution or via
    /// </summary>
    public interface ICanLintMyself : IHaveSolution
    {
        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The files to lint, if not given lints all files", Separator = " ")]
        public IEnumerable<string> LintFiles => GetInjectionValue(() => LintFiles ?? Array.Empty<string>());

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

    /// <summary>
    /// Defines use of a git repository
    /// </summary>
    /// <remarks>
    /// This explicitly excludes the attribute so that it can be defined in the consumers build
    /// </remarks>
    public interface IHaveGitRepository
    {
        /// <summary>
        /// The Git Repository currently being built
        /// </summary>
        GitRepository? GitRepository { get; }
    }

    /// <summary>
    /// Defines use of GitVersion
    /// </summary>
    public interface IHaveGitVersion : IHaveGitRepository
    {
        /// <summary>
        /// The current version as defined by GitVersion 
        /// </summary>
        GitVersion GitVersion { get; }
    }

    /// <summary>
    /// A tool to update the readme
    /// </summary>
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

    /// <summary>
    /// Defines the artifacts output directory.
    /// </summary>
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

    /// <summary>
    /// Defines the output directory
    /// </summary>
    public interface IOutputArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where packaged output should be placed (zip, webdeploy, etc)
        /// </summary>
        public AbsolutePath OutputDirectory => ArtifactsDirectory / "output";
    }

    /// <summary>
    /// Defines the publish output directory, this is used to staged published applications and so on.
    /// </summary>
    public interface IPublishArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where publish output should be placed
        /// </summary>
        public AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";
    }

    /// <summary>
    /// Defines a logs directory where structured build and other logs can be placed.
    /// </summary>
    public interface IOutputLogs : IHaveArtifacts
    {
        /// <summary>
        /// The directory where logs will be placed
        /// </summary>
        public AbsolutePath LogsDirectory => ArtifactsDirectory / "logs";
    }

    /// <summary>
    /// Defines the test result artifacts locations
    /// </summary>
    /// <remarks>
    /// Used for things like xunit test result files for publish to azure devops or otherwise.
    /// </remarks>
    public interface IOutputTestArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where test results will be placed
        /// </summary>
        public AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test";
    }

    /// <summary>
    /// Defines a directory for nuget packages that should be pushed should go into
    /// </summary>
    public interface IOutputNuGetArtifacts : IHaveArtifacts
    {
        /// <summary>
        /// The directory where nuget packages will be placed
        /// </summary>
        public AbsolutePath NuGetPackageDirectory => ArtifactsDirectory / "nuget";
    }

    /// <summary>
    /// Adds a code coverage directory
    /// </summary>
    /// <remarks>
    /// This directory is left separate to allow easier integration with editors that might look it's contents to display coverage.
    /// </remarks>
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

    /// <summary>
    /// A common sample directory
    /// </summary>
    public interface IIncludeSamples
    {
        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SampleDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "sample",
            NukeBuild.RootDirectory / "samples"
        );
    }

    /// <summary>
    /// Extensions related to file paths
    /// </summary>
    public static class FilePathExtensions
    {
        private static readonly ConcurrentDictionary<AbsolutePath, AbsolutePath> _cache =
            new ConcurrentDictionary<AbsolutePath, AbsolutePath>();

        /// <summary>
        /// Returns the first directory that exists on disk
        /// </summary>
        /// <remarks>
        /// Caches the result for faster lookups later
        /// </remarks>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static AbsolutePath PickDirectory(params AbsolutePath[] paths)
        {
            foreach (var path in paths)
            {
                if (_cache.TryGetValue(path, out _))
                    return path;
                if (!DirectoryExists(path))
                {
                    continue;
                }

                foreach (var p in paths)
                    _cache.TryAdd(p, path);
                return path;
            }

            return paths.First();
        }
    }

    /// <summary>
    /// The directory where sources should be placed.
    /// </summary>
    public interface IIncludeSources
    {
        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SourceDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "src",
            NukeBuild.RootDirectory / "source",
            NukeBuild.RootDirectory / "sources"
        );
    }

    /// <summary>
    /// The directory where templates should be placed.
    /// </summary>
    public interface IIncludeTemplates
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public AbsolutePath TemplatesDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "template",
            NukeBuild.RootDirectory / "templates"
        );
    }

    /// <summary>
    /// The directory where tests should be placed.
    /// </summary>
    public interface IIncludeTests
    {
        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestsDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "test",
            NukeBuild.RootDirectory / "tests"
        );
    }

    /// <summary>
    /// Includes a force flag that can be used to ensure caches or the disk is cleaned up more than is normally required
    /// </summary>
    public interface IMayTheForceBeWithYou
    {
        /// <summary>
        /// Force a clean build, otherwise leave some incremental build pieces
        /// </summary>
        [Parameter("Force a clean build")]
        public bool Force { get; }
    }

    /// <summary>
    /// Defines a build version target
    /// </summary>
    public interface IHaveBuildVersion : IHaveGitVersion, IHaveSolution, IHaveConfiguration
    {
        /// <summary>
        /// prints the build information.
        /// </summary>
        public Target BuildVersion => _ => _
           .Executes(
                () =>
                {
                    Logger.Info(
                        "Building version {0} of {1} ({2}) using version {3} of Nuke.",
                        GitVersion?.NuGetVersionV2 ?? GitVersion?.NuGetVersion,
                        Solution.Name,
                        Configuration,
                        typeof(NukeBuild).Assembly.GetVersionText()
                    );
                }
            );
    }

    /// <summary>
    /// Defines a solution
    /// </summary>
    public interface IHaveSolution
    {
        /// <summary>
        /// The solution currently being build
        /// </summary>
        [Solution]
        public Solution Solution => GetInjectionValue(() => Solution);
    }

    /// <summary>
    /// Defines a clean target
    /// </summary>
    public interface IHaveCleanTarget
    {
        /// <summary>
        /// The Clean Target
        /// </summary>
        Target Clean { get; }
    }

    /// <summary>
    /// Defines a target that cleans common directories
    /// </summary>
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

                    // ReSharper disable SuspiciousTypeConversion.Global
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

                        if (this is IIncludeTests tests && DirectoryExists(tests.TestsDirectory))
                        {
                            tests.TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }
                    }
                } // ReSharper restore SuspiciousTypeConversion.Global
            );
    }

    /// <summary>
    /// Defines a task that generates a code coverage report from a given set of report documents
    /// </summary>
    public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports
    {
        /// <summary>
        /// The directory where the report will be places
        /// </summary>
        public AbsolutePath CoverageReportDirectory => CoverageDirectory / "report";

        /// <summary>
        /// Generates a code coverage report got the given set of input reports
        /// </summary>
        public Target Generate_Code_Coverage_Report => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .OnlyWhenDynamic(() => InputReports.Any())
           .Executes(
                () => ReportGenerator(
                    s => WithTag(s)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageReportDirectory)
                       .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                )
            );
    }

    /// <summary>
    /// Generates a code coverage summary
    /// </summary>
    public interface IGenerateCodeCoverageSummary : ITriggerCodeCoverageReports
    {
        /// <summary>
        /// The directory where the summary will be places
        /// </summary>
        public AbsolutePath CoverageSummaryDirectory => CoverageDirectory / "summary";

        /// <summary>
        /// Generate a code coverage summary for the given reports
        /// </summary>
        public Target Generate_Code_Coverage_Summary => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .OnlyWhenDynamic(() => InputReports.Any())
           .Executes(
                () => ReportGenerator(
                    s => WithTag(s)
                        // .SetToolPath(toolPath)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageSummaryDirectory)
                       .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                )
            );
    }

    /// <summary>
    /// Generates a code coverage badges
    /// </summary>
    public interface IGenerateCodeCoverageBadges : ITriggerCodeCoverageReports
    {
        /// <summary>
        /// The directory where the badges will be places
        /// </summary>
        public AbsolutePath CoverageBadgeDirectory => CoverageDirectory / "badges";

        /// <summary>
        /// Generate a code coverage badges for the given reports
        /// </summary>
        public Target Generate_Code_Coverage_Badges => _ => _
           .After(Generate_Code_Coverage_Report_Cobertura)
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .OnlyWhenDynamic(() => InputReports.Any())
           .Executes(
                () => ReportGenerator(
                    s => WithTag(s)
                        // .SetToolPath(toolPath)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageBadgeDirectory)
                       .SetReportTypes(ReportTypes.Badges)
                )
            );
    }

    /// <summary>
    /// Triggers code coverage to happen
    /// </summary>
    /// <remarks>
    /// This causes code coverage to trigger
    /// </remarks>
    public interface ITriggerCodeCoverageReports : IIncludeCodeCoverage, IHaveTestTarget
    {
        /// <summary>
        /// The input reports
        /// </summary>
        /// <remarks>
        /// used to determine if any coverage was emitted, if not the tasks will skip to avoid errors
        /// </remarks>
        public IEnumerable<string> InputReports => CoverageDirectory
           .GlobFiles("**/*.cobertura.xml")
           .Select(z => z.ToString());

        /// <summary>
        /// ensures that ReportGenerator is called with the appropriate settings given the current state.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected ReportGeneratorSettings WithTag(ReportGeneratorSettings settings)
        {
            settings = settings.SetToolPath(
                ToolPathResolver.GetPackageExecutable(
                    "ReportGenerator",
                    "ReportGenerator.dll",
                    framework: "netcoreapp3.0"
                )
            );

            return this switch
            {
                IHaveGitVersion gitVersion => settings.SetTag(gitVersion.GitVersion.InformationalVersion),
                IHaveGitRepository gitRepository when gitRepository.GitRepository != null => settings.SetTag(
                    gitRepository.GitRepository.Head
                ),
                _ => settings
            };
        }

        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Trigger_Code_Coverage_Reports => _ => _
           .TriggeredBy(Test)
           .After(Test)
           .Description("Generates code coverage reports")
           .Unlisted()
           .OnlyWhenDynamic(() => InputReports.Any());

        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Generate_Code_Coverage_Report_Cobertura => _ => _
           .TriggeredBy(Trigger_Code_Coverage_Reports)
           .Unlisted()
           .OnlyWhenDynamic(() => InputReports.Any())
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
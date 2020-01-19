using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.Readme;
using Temp.CleanupCode;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Temp.CleanupCode.CleanupCodeTasks;
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Base build plan and tasks
    /// </summary>
    [PublicAPI]
    [DotNetVerbosityMapping]
    [MSBuildVerbosityMapping]
    [NuGetVerbosityMapping]
    public abstract class RocketBoosterBuild<T> : NukeBuild, IRocketBoosterBuild<T>
        where T : Configuration
    {
        protected RocketBoosterBuild(Func<T> configurationDefault)
        {
            Configuration = configurationDefault();
        }

        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The files to lint, if not given lints all files", Separator = " ")]
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] LintFiles { get; set; } = Array.Empty<string>();
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The profile to use for linting")]
        public string LintProfile { get; set; } = "Full Cleanup";

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

        /// <summary>
        /// Configuration to build - Default is 'Debug' (local) or 'Release' (server)
        /// </summary>
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public T Configuration { get; }

        /// <summary>
        /// Force a clean build, otherwise leave some incremental build pieces
        /// </summary>
        [Parameter("Force a clean build")]
        public bool Force { get; }

        /// <summary>
        /// The solution currently being build
        /// </summary>
        [Solution]
        public Solution Solution { get; }

        /// <summary>
        /// The Git Repository currently being built
        /// </summary>
        [GitRepository]
        public GitRepository? GitRepository { get; }

        /// <summary>
        /// The Git Version information either computed by GitVersion itself, or as defined by environment variables of the format
        /// `GITVERSION_*`
        /// </summary>
        [ComputedGitVersion]
        public GitVersion? GitVersion { get; }

        /// <summary>
        /// The readme updater that ensures that all the badges are in sync.
        /// </summary>
        [Readme]
        public ReadmeUpdater Readme { get; }

        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SampleDirectory => RootDirectory / "sample";

        /// <summary>
        /// The directory where sources will be placed
        /// </summary>
        public AbsolutePath SourceDirectory => RootDirectory / "src";

        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public AbsolutePath TemplatesDirectory => RootDirectory / "templates";

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestDirectory => DirectoryExists(RootDirectory / "tests")
            ? RootDirectory / "tests"
            : RootDirectory / "test";

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestsDirectory => TestDirectory;

        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
        public AbsolutePath ArtifactsDirectory { get; } =
            GetVariable<AbsolutePath>("Artifacts") ?? RootDirectory / "artifacts";

        /// <summary>
        /// The directory where logs will be placed
        /// </summary>
        public AbsolutePath LogsDirectory => ArtifactsDirectory / "logs";

        /// <summary>
        /// The directory where test results will be placed
        /// </summary>
        public AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test";

        /// <summary>
        /// The directory where nuget packages will be placed
        /// </summary>
        public AbsolutePath NuGetPackageDirectory => ArtifactsDirectory / "nuget";

        /// <summary>
        /// The directory where publish output should be placed
        /// </summary>
        public AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";

        /// <summary>
        /// The directory where packaged output should be placed (zip, webdeploy, etc)
        /// </summary>
        public AbsolutePath OutputDirectory => ArtifactsDirectory / "output";

        /// <summary>
        /// The directory where coverage artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
        public AbsolutePath CoverageDirectory { get; } =
            GetVariable<AbsolutePath>("Coverage") ?? RootDirectory / "coverage";

        /// <summary>
        /// prints the build information.
        /// </summary>
        public Target BuildVersion => _ => _
           .Executes(
                () =>
                {
                    Logger.Info(
                        "Building version {0} of {1} ({2}) using version {3} of Nuke.",
                        GitVersion.NuGetVersionV2 ?? GitVersion.NuGetVersion,
                        Solution.Name,
                        Configuration,
                        typeof(NukeBuild).Assembly.GetVersionText()
                    );
                }
            );

        /// <summary>
        /// clean all artifact directories
        /// </summary>
        public Target Clean => _ => _
           .DependsOn(BuildVersion)
           .Executes(
                () =>
                {
                    EnsureCleanDirectory(ArtifactsDirectory);
                    EnsureExistingDirectory(LogsDirectory);
                    EnsureExistingDirectory(TestResultsDirectory);
                    EnsureExistingDirectory(NuGetPackageDirectory);
                    EnsureExistingDirectory(PublishDirectory);
                    EnsureExistingDirectory(OutputDirectory);
                    EnsureCleanDirectory(CoverageDirectory);

                    if (Force)
                    {
                        if (DirectoryExists(SampleDirectory))
                        {
                            SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (DirectoryExists(SourceDirectory))
                        {
                            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (DirectoryExists(TemplatesDirectory))
                        {
                            TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (DirectoryExists(TestDirectory))
                        {
                            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }
                    }
                }
            );


        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Generate_Code_Coverage_Reports => _ => _
           .Description("Generates code coverage reports")
           .Unlisted()
           .OnlyWhenDynamic(() => CoverageDirectory.GlobFiles("**/*.cobertura.xml").Count > 0)
           .Executes(
                () =>
                {
                    var reports = CoverageDirectory.GlobFiles("**/*.cobertura.xml").Select(z => z.ToString());
                    // TEMP work around for issue in nuke
                    var toolPath =
#if NETSTANDARD2_1
                    ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
#else
                        ToolPathResolver.GetPackageExecutable(
                            "ReportGenerator",
                            "ReportGenerator.dll",
                            framework: "netcoreapp2.1"
                        );
#endif
                    ReportGenerator(
                        s => s
                           .SetToolPath(toolPath)
                           .SetReports(reports)
                           .SetTargetDirectory(CoverageDirectory / "report")
                           .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                           .SetTag(GitVersion.InformationalVersion)
                    );

                    ReportGenerator(
                        s => s
                           .SetToolPath(toolPath)
                           .SetReports(reports)
                           .SetTargetDirectory(CoverageDirectory)
                           .SetReportTypes(ReportTypes.Cobertura)
                           .SetTag(GitVersion.InformationalVersion)
                    );

                    ReportGenerator(
                        s => s
                           .SetToolPath(toolPath)
                           .SetReports(reports)
                           .SetTargetDirectory(CoverageDirectory / "badges")
                           .SetReportTypes(ReportTypes.Badges)
                           .SetTag(GitVersion.InformationalVersion)
                    );

                    ReportGenerator(
                        s => s
                           .SetToolPath(toolPath)
                           .SetReports(reports)
                           .SetTargetDirectory(CoverageDirectory / "summary")
                           .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                           .SetTag(GitVersion.InformationalVersion)
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

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while
        /// keeping the rest of the readme correct.
        /// </summary>
        public Target GenerateReadme => _ => _
           .Unlisted()
           .OnlyWhenDynamic(
                () => IsLocalBuild && ( Force || InvokedTargets.Any(z => z.Name == nameof(GenerateReadme)) ||
                    ExecutingTargets.Any(z => z.Name == nameof(GenerateReadme)) )
            )
           .Executes(
                () =>
                {
                    var readmeContent = File.ReadAllText(RootDirectory / "Readme.md");
                    readmeContent = Readme.Process(readmeContent, this);
                    File.WriteAllText(RootDirectory / "Readme.md", readmeContent);
                }
            );
    }

    /// <summary>
    /// Base build plan and tasks.
    /// </summary>
    /// <seealso cref="NukeBuild" />
    /// <seealso cref="IRocketBoosterBuild{T}" />
    public abstract class RocketBoosterBuild : RocketBoosterBuild<Configuration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RocketBoosterBuild"/> class.
        /// </summary>
        protected RocketBoosterBuild()
            : base(() => IsLocalBuild ? Configuration.Debug : Configuration.Release) { }
    }
}
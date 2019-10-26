using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.Tools.ReportGenerator;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Nuke.Common.Tools.VSWhere.VSWhereTasks;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common.Utilities;
using Rocket.Surgery.Nuke.Readme;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Base build plan and tasks
    /// </summary>
    [PublicAPI]
    [DotNetVerbosityMapping, MSBuildVerbosityMapping, NuGetVerbosityMapping]
    public abstract class RocketBoosterBuild : NukeBuild
    {
        /// <summary>
        /// Configuration to build - Default is 'Debug' (local) or 'Release' (server)
        /// </summary>
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        /// <summary>
        /// Force a clean build, otherwise leave some incremental build pieces
        /// </summary>
        [Parameter("Force a clean build")]
        public readonly bool Force;

        /// <summary>
        /// The solution currently being build
        /// </summary>
        [Solution] public readonly Solution Solution;

        /// <summary>
        /// The Git Repository currently being built
        /// </summary>
        [GitRepository] public readonly GitRepository GitRepository;

        /// <summary>
        /// The Git Version information either computed by GitVersion itself, or as defined by environment variables of the format `GITVERSION_*`
        /// </summary>
        [ComputedGitVersion] public readonly GitVersion GitVersion;

        /// <summary>
        /// The readme updater that ensures that all the badges are in sync.
        /// </summary>

        [Readme] public readonly ReadmeUpdater Readme;

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
        public AbsolutePath TestDirectory => DirectoryExists(RootDirectory / "tests") ? RootDirectory / "tests" : RootDirectory / "test";

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        public AbsolutePath TestsDirectory => TestDirectory;

        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
        public readonly AbsolutePath ArtifactsDirectory = GetVariable<AbsolutePath>("Artifacts") ?? RootDirectory / "artifacts";

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
        public readonly AbsolutePath CoverageDirectory = GetVariable<AbsolutePath>("Coverage") ?? RootDirectory / "coverage";

        /// <summary>
        /// prints the build information.
        /// </summary>
        public Target BuildVersion => _ => _
            .Executes(() =>
            {
                Logger.Info("Building version {0} of {1} ({2}) using version {3} of Nuke.",
                    GitVersion.FullSemVer,
                    Solution.Name,
                    Configuration,
                    typeof(NukeBuild).Assembly.GetVersionText());
            });

        /// <summary>
        /// clean all artifact directories
        /// </summary>
        public Target Clean => _ => _
            .DependsOn(BuildVersion)
            .Executes(() =>
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
                    SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                    SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                    TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                    TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                }
            });


        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        public Target Generate_Code_Coverage_Reports => _ => _
            .Description("Generates code coverage reports")
            .Unlisted()
            .OnlyWhenDynamic(() => CoverageDirectory.GlobFiles("**/*.cobertura.xml").Count > 0)
            .Executes(() =>
            {
                var reports = CoverageDirectory.GlobFiles("**/*.cobertura.xml").Select(z => z.ToString());
                // TEMP work around for issue in nuke
                var toolPath =
#if NETSTANDARD2_1
                    ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.exe", framework: "netcoreapp3.0");
#else
                    ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.exe", framework: "net47");
#endif
                ReportGenerator(s => s
                    .SetToolPath(toolPath)
                    .SetReports(reports)
                        .SetTargetDirectory(CoverageDirectory / "report")
                        .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                        .SetTag(GitVersion.InformationalVersion)
                    );

                ReportGenerator(s => s
                    .SetToolPath(toolPath)
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory)
                    .SetReportTypes(ReportTypes.Cobertura)
                    .SetTag(GitVersion.InformationalVersion)
                );

                ReportGenerator(s => s
                    .SetToolPath(toolPath)
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory / "badges")
                    .SetReportTypes(ReportTypes.Badges)
                    .SetTag(GitVersion.InformationalVersion)
                );

                ReportGenerator(s => s
                    .SetToolPath(toolPath)
                    .SetReports(reports)
                    .SetTargetDirectory(CoverageDirectory / "summary")
                    .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                    .SetTag(GitVersion.InformationalVersion)
                );
                CopyFile(CoverageDirectory / "Cobertura.xml", CoverageDirectory / "solution.cobertura", FileExistsPolicy.OverwriteIfNewer);
                RenameFile(CoverageDirectory / "Cobertura.xml", "solution.xml", FileExistsPolicy.OverwriteIfNewer);
            });

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while keeping the rest of the readme correct.
        /// </summary>
        public Target GenerateReadme => _ => _
            .Unlisted()
            .OnlyWhenDynamic(() => IsLocalBuild && (Force || InvokedTargets.Any(z => z.Name == nameof(GenerateReadme))))
            .Executes(() =>
            {
                var readmeContent = File.ReadAllText(RootDirectory / "Readme.md");
                readmeContent = Readme.Process(readmeContent, this);
                File.WriteAllText(RootDirectory / "Readme.md", readmeContent);
            });
    }
}

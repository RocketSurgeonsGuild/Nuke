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

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Base build plan and tasks
    /// </summary>
    [PublicAPI]
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
        public AbsolutePath TestDirectory => RootDirectory / "test";

        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
        public readonly AbsolutePath ArtifactsDirectory = Variable("Artifacts") != null ? (AbsolutePath)Variable("Artifacts") : RootDirectory / "artifacts";

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
        /// The directory where coverage artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
        public readonly AbsolutePath CoverageDirectory = Variable("Coverage") != null ? (AbsolutePath)Variable("Coverage") : RootDirectory / "coverage";

        /// <summary>
        /// prints the build information.
        /// </summary>
        public Target BuildVersion => _ => _
            .Executes(() =>
            {
                Logger.Info("Building version {0} of {1} ({2}) using version {3} of Nuke.",
                    GitVersion.NuGetVersionV2 ?? GitVersion.NuGetVersion,
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
                EnsureCleanDirectory(CoverageDirectory);

                if (Force)
                {
                    EnsureExistingDirectory(SampleDirectory);
                    SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

                    EnsureExistingDirectory(SourceDirectory);
                    SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

                    EnsureExistingDirectory(TemplatesDirectory);
                    TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);

                    EnsureExistingDirectory(TestDirectory);
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
                CopyFile(CoverageDirectory / "Cobertura.xml", CoverageDirectory / "solution.cobertura");
                RenameFile(CoverageDirectory / "Cobertura.xml", "solution.xml");
            });

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while keeping the rest of the readme correct.
        /// </summary>
        public Target GenerateReadme => _ => _
            .Unlisted()
            .TriggeredBy(Clean)
            .OnlyWhenDynamic(() => IsLocalBuild && Force)
            .Executes(() =>
            {
                var readmeContent = File.ReadAllText(RootDirectory / "Readme.md");
                readmeContent = Readme.Process(readmeContent, this);
                File.WriteAllText(RootDirectory / "Readme.md", readmeContent);
            });
    }
}

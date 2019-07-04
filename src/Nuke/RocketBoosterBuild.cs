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
}

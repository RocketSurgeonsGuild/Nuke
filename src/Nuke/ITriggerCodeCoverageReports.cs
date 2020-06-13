using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Triggers code coverage to happen
    /// </summary>
    /// <remarks>
    /// This causes code coverage to trigger
    /// </remarks>
    public interface ITriggerCodeCoverageReports : IHaveCodeCoverage, IHaveTestTarget, ITrigger
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
                    ReportGeneratorTasks.ReportGenerator(
                        s => WithTag(s)
                            // .SetToolPath(toolPath)
                           .SetReports(InputReports)
                           .SetTargetDirectory(CoverageDirectory)
                           .SetReportTypes(ReportTypes.Cobertura)
                    );

                    FileSystemTasks.CopyFile(
                        CoverageDirectory / "Cobertura.xml",
                        CoverageDirectory / "solution.cobertura",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                    FileSystemTasks.CopyFile(
                        CoverageDirectory / "Cobertura.xml",
                        CoverageDirectory / "solution.xml",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                    FileSystemTasks.RenameFile(
                        CoverageDirectory / "solution.xml",
                        CoverageDirectory / "cobertura.xml",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                }
            );

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
                var _ => settings
            };
        }
    }
}
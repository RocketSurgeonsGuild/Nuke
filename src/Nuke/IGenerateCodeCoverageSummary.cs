using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke
{
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
                () => ReportGeneratorTasks.ReportGenerator(
                    s => WithTag(s)
                        // .SetToolPath(toolPath)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageSummaryDirectory)
                       .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                )
            );
    }
}
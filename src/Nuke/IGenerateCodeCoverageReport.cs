using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines a task that generates a code coverage report from a given set of report documents
    /// </summary>
    public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports, IGenerate
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
                () => ReportGeneratorTasks.ReportGenerator(
                    s => WithTag(s)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageReportDirectory)
                       .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                )
            );
    }
}
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke
{
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
                () => ReportGeneratorTasks.ReportGenerator(
                    s => WithTag(s)
                        // .SetToolPath(toolPath)
                       .SetReports(InputReports)
                       .SetTargetDirectory(CoverageBadgeDirectory)
                       .SetReportTypes(ReportTypes.Badges)
                )
            );
    }
}
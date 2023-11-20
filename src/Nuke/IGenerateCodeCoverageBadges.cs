using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Generates a code coverage badges
/// </summary>
public interface IGenerateCodeCoverageBadges : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the badges will be places
    /// </summary>
    public AbsolutePath CoverageBadgeDirectory => CoverageDirectory / "badges";

    /// <summary>
    ///     Generate a code coverage badges for the given reports
    /// </summary>
    public Target GenerateCodeCoverageBadges => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () => ReportGeneratorTasks.ReportGenerator(
                                                             s => WithTag(s)
                                                                 // .SetToolPath(toolPath)
                                                                 .SetFramework(Constants.ReportGeneratorFramework)
                                                                 .SetReports(InputReports)
                                                                 .SetTargetDirectory(CoverageBadgeDirectory)
                                                                 .SetReportTypes(ReportTypes.Badges)
                                                         )
                                                     );
}

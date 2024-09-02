using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;
using Rocket.Surgery.Nuke.GithubActions;

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
    [NonEntryTarget]
    public Target GenerateCodeCoverageBadges => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () => ReportGeneratorTasks.ReportGenerator(
                                                             s => Defaults(s)
                                                                 .SetTargetDirectory(CoverageBadgeDirectory)
                                                                 .SetReportTypes(ReportTypes.Badges)
                                                         )
                                                     );
}

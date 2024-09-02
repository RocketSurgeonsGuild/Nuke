using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Generates a code coverage summary
/// </summary>
public interface IGenerateCodeCoverageSummary : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the summary will be places
    /// </summary>
    public AbsolutePath CoverageSummaryDirectory => CoverageDirectory / "summary";

    /// <summary>
    ///     Generate a code coverage summary for the given reports
    /// </summary>
    [NonEntryTarget]
    public Target GenerateCodeCoverageSummary => d => d
                                                     .After(GenerateCodeCoverageReportCobertura)
                                                     .TriggeredBy(TriggerCodeCoverageReports)
                                                     .Unlisted()
                                                     .OnlyWhenDynamic(() => InputReports.Any())
                                                     .Executes(
                                                          () => ReportGeneratorTasks.ReportGenerator(
                                                              s => Defaults(s)
                                                                  .SetTargetDirectory(CoverageSummaryDirectory)
                                                                  .SetReportTypes(
                                                                       ReportTypes.HtmlSummary,
                                                                       ReportTypes.TextSummary,
                                                                       ReportTypes.MarkdownSummary
                                                                   )
                                                          )
                                                      );
}

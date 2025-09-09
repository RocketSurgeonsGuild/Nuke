using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Generates a code coverage summary
/// </summary>
[PublicAPI]
public interface IGenerateCodeCoverageSummary : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the summary will be places
    /// </summary>
    AbsolutePath CoverageSummaryDirectory => CoverageDirectory / "summary";

    /// <summary>
    ///     Generate a code coverage summary for the given reports
    /// </summary>
    [NonEntryTarget]
    Target GenerateCodeCoverageSummary => d => d
                                                         .After(GenerateCodeCoverageReportCobertura)
                                                         .TriggeredBy(CollectCodeCoverage)
                                                         .Unlisted()
                                                         .AssuredAfterFailure()
                                                         .ProceedAfterFailure()
                                                         .OnlyWhenDynamic(() => InputReports.Any())
                                                         .Executes(
                                                              () => ReportGeneratorTasks.ReportGenerator(
                                                                  s => Defaults(s)
                                                                      .SetTargetDirectory(CoverageSummaryDirectory)
                                                                      .SetReportTypes(
                                                                           ReportTypes.HtmlSummary,
                                                                           ReportTypes.TextSummary,
                                                                           ReportTypes.MarkdownSummary,
                                                                           ReportTypes.MarkdownSummaryGithub
                                                                       )
                                                              )
                                                          );
}

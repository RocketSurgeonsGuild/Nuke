using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

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
    public Target GenerateCodeCoverageSummary => d => d
                                                     .After(GenerateCodeCoverageReportCobertura)
                                                     .TriggeredBy(TriggerCodeCoverageReports)
                                                     .Unlisted()
                                                     .OnlyWhenDynamic(() => InputReports.Any())
                                                     .Executes(
                                                          () =>
                                                          {
                                                              var output = ReportGeneratorTasks.ReportGenerator(
                                                                  s => WithTag(s)
                                                                      .SetFramework(Constants.ReportGeneratorFramework)
                                                                      // .SetToolPath(toolPath)
                                                                      .SetReports(InputReports)
                                                                      .SetTargetDirectory(CoverageSummaryDirectory)
                                                                      .SetReportTypes(
                                                                           ReportTypes.HtmlSummary, ReportTypes.TextSummary, ReportTypes.MarkdownSummary
                                                                       )
                                                              );

                                                              return output;
                                                          }
                                                      );
}

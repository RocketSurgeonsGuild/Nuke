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


    [Obsolete("Legacy target has been renamed to GenerateCodeCoverageSummary")]
    // ReSharper disable once InconsistentNaming
#pragma warning disable CS1591, CA1707
    public Target Generate_Code_Coverage_Summary =>
        d => d.DependsOn(GenerateCodeCoverageSummary).Unlisted();
#pragma warning restore CS1591, CA1707

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

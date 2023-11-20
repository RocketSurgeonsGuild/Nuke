using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a task that generates a code coverage report from a given set of report documents
/// </summary>
public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the report will be places
    /// </summary>
    public AbsolutePath CoverageReportDirectory => CoverageDirectory / "report";

    /// <summary>
    ///     Generates a code coverage report got the given set of input reports
    /// </summary>
    public Target GenerateCodeCoverageReport => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () =>
                                                         {
                                                             return ReportGeneratorTasks.ReportGenerator(
                                                                 s => WithTag(s)
                                                                     .SetFramework(Constants.ReportGeneratorFramework)
                                                                     .SetReports(InputReports)
                                                                     .SetTargetDirectory(CoverageReportDirectory)
                                                                     .SetReportTypes(
                                                                          ReportTypes.HtmlInline_AzurePipelines_Dark
                                                                      )
                                                             );
                                                         }
                                                     );
}

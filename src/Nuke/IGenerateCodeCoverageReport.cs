using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a task that generates a code coverage report from a given set of report documents
/// </summary>
[PublicAPI]
public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the report will be placed
    /// </summary>
    public AbsolutePath CoverageReportDirectory => CoverageDirectory / "report";

    /// <summary>
    ///     Generates a code coverage report got the given set of input reports
    /// </summary>
    [NonEntryTarget]
    public Target GenerateCodeCoverageReport => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(CollectCodeCoverage)
                                                    .Unlisted()
                                                    .AssuredAfterFailure()
                                                    .ProceedAfterFailure()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () => ReportGeneratorTasks.ReportGenerator(
                                                             s => Defaults(s)
                                                                 .SetTargetDirectory(CoverageReportDirectory)
                                                                  // these trample... don't forget next time.
                                                                 .SetReportTypes( /*ReportTypes.HtmlInline_AzurePipelines_Dark, */ReportTypes.Html_Dark)
                                                         )
                                                     );
}

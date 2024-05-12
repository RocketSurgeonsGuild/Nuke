using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a task that generates a code coverage report from a given set of report documents
/// </summary>
public interface IGenerateCodeCoverageReport : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     Generates a code coverage report got the given set of input reports
    /// </summary>
    public Target GenerateCodeCoverageReport => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () => ReportGeneratorTasks.ReportGenerator(
                                                             s => Defaults(s)
                                                                .SetReportTypes(ReportTypes.HtmlInline_AzurePipelines_Dark)
                                                         )
                                                     );
}
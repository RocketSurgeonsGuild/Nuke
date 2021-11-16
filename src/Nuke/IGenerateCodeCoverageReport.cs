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

    [Obsolete("Legacy target has been renamed to GenerateCodeCoverageReport")]
    // ReSharper disable once InconsistentNaming
#pragma warning disable CS1591, CA1707
    public Target Generate_Code_Coverage_Report =>
        _ => _.DependsOn(GenerateCodeCoverageReport).Unlisted();
#pragma warning restore CS1591, CA1707

    /// <summary>
    ///     Generates a code coverage report got the given set of input reports
    /// </summary>
    public Target GenerateCodeCoverageReport => _ => _
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () =>
                                                         {
                                                             return ReportGeneratorTasks.ReportGenerator(
                                                                 s => WithTag(s)
                                                                     .SetFramework("netcoreapp3.1")
                                                                     .SetReports(InputReports)
                                                                     .SetTargetDirectory(CoverageReportDirectory)
                                                                     .SetReportTypes(
                                                                          ReportTypes.HtmlInline_AzurePipelines_Dark
                                                                      )
                                                             );
                                                         }
                                                     );
}

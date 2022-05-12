using Nuke.Common.CI.GitHubActions;
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
        _ => _.DependsOn(GenerateCodeCoverageSummary).Unlisted();
#pragma warning restore CS1591, CA1707

    /// <summary>
    ///     Generate a code coverage summary for the given reports
    /// </summary>
    public Target GenerateCodeCoverageSummary => _ => _
                                                     .After(GenerateCodeCoverageReportCobertura)
                                                     .TriggeredBy(TriggerCodeCoverageReports)
                                                     .Unlisted()
                                                     .OnlyWhenDynamic(() => InputReports.Any())
                                                     .Executes(
                                                          () => ReportGeneratorTasks.ReportGenerator(
                                                              s => WithTag(s)
                                                                  .SetFramework(Constants.ReportGeneratorFramework)
                                                                   // .SetToolPath(toolPath)
                                                                  .SetReports(InputReports)
                                                                  .SetTargetDirectory(CoverageSummaryDirectory)
                                                                  .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.TextSummary)
                                                          )
                                                      );
}

/// <summary>
///     Generates a code coverage summary for github actions
/// </summary>
public interface IGenerateCodeCoverageSummaryForGithubActions : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     Generate a code coverage summary for the given reports
    /// </summary>
    public Target GenerateCodeCoverageSummaryForGithubActions => _ => _
                                                                     .After(GenerateCodeCoverageReportCobertura)
                                                                     .TriggeredBy(TriggerCodeCoverageReports)
                                                                     .Unlisted()
                                                                     .OnlyWhenDynamic(() => InputReports.Any())
                                                                     .OnlyWhenStatic(() => IsServerBuild)
                                                                     .OnlyWhenStatic(
                                                                          () => Host == GitHubActions.Instance && !string.IsNullOrWhiteSpace(
                                                                              EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY", null)
                                                                          )
                                                                      )
                                                                     .Executes(
                                                                          () =>
                                                                          {
                                                                              var temp = TemporaryDirectory / "github-actions-summary";
                                                                              FileSystemTasks.EnsureCleanDirectory(temp);
                                                                              var output = ReportGeneratorTasks.ReportGenerator(
                                                                                  s => WithTag(s)
                                                                                      .SetFramework(Constants.ReportGeneratorFramework)
                                                                                       // .SetToolPath(toolPath)
                                                                                      .SetReports(InputReports)
                                                                                      .SetTargetDirectory(temp)
                                                                                      .SetReportTypes(ReportTypes.MarkdownSummary)
                                                                              );
                                                                              FileSystemTasks.CopyFile(
                                                                                  temp / "summary.md",
                                                                                  EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY")
                                                                              );
                                                                              return output;
                                                                          }
                                                                      );
}

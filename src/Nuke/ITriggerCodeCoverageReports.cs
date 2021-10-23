using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.ReportGenerator;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Triggers code coverage to happen
/// </summary>
/// <remarks>
///     This causes code coverage to trigger
/// </remarks>
public interface ITriggerCodeCoverageReports : IHaveCodeCoverage, IHaveTestTarget, ITrigger
{
    /// <summary>
    ///     The input reports
    /// </summary>
    /// <remarks>
    ///     used to determine if any coverage was emitted, if not the tasks will skip to avoid errors
    /// </remarks>
    public IEnumerable<string> InputReports => CoverageDirectory
                                              .GlobFiles("**/*.cobertura.xml")
                                              .Select(z => z.ToString());

    [Obsolete("Legacy target has been renamed to TriggerCodeCoverageReports")]
    // ReSharper disable once InconsistentNaming
#pragma warning disable CS1591, CA1707
    public Target Trigger_Code_Coverage_Reports => _ => _.DependsOn(TriggerCodeCoverageReports).Unlisted();
#pragma warning restore CS1591, CA1707

    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    public Target TriggerCodeCoverageReports => _ => _
                                                    .TriggeredBy(Test)
                                                    .After(Test)
                                                    .Description("Generates code coverage reports")
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any());

    [Obsolete("Legacy target has been renamed to GenerateCodeCoverageReportCobertura")]
    // ReSharper disable once InconsistentNaming
#pragma warning disable CS1591, CA1707
    public Target Generate_Code_Coverage_Report_Cobertura =>
        _ => _.DependsOn(GenerateCodeCoverageReportCobertura).Unlisted();
#pragma warning restore CS1591, CA1707


    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    public Target GenerateCodeCoverageReportCobertura => _ => _
                                                             .TriggeredBy(TriggerCodeCoverageReports)
                                                             .Unlisted()
                                                             .OnlyWhenDynamic(() => InputReports.Any())
                                                             .Executes(
                                                                  () =>
                                                                  {
                                                                      // var toolPath = ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
                                                                      ReportGeneratorTasks.ReportGenerator(
                                                                          s => WithTag(s)
                                                                               // .SetToolPath(toolPath)
                                                                              .SetReports(InputReports)
                                                                              .SetTargetDirectory(CoverageDirectory)
                                                                              .SetReportTypes(ReportTypes.Cobertura)
                                                                      );

                                                                      FileSystemTasks.CopyFile(
                                                                          CoverageDirectory / "Cobertura.xml",
                                                                          CoverageDirectory / "solution.cobertura",
                                                                          FileExistsPolicy.OverwriteIfNewer
                                                                      );
                                                                      FileSystemTasks.CopyFile(
                                                                          CoverageDirectory / "Cobertura.xml",
                                                                          CoverageDirectory / "solution.xml",
                                                                          FileExistsPolicy.OverwriteIfNewer
                                                                      );
                                                                      FileSystemTasks.RenameFile(
                                                                          CoverageDirectory / "solution.xml",
                                                                          CoverageDirectory / "cobertura.xml",
                                                                          FileExistsPolicy.OverwriteIfNewer
                                                                      );
                                                                  }
                                                              );

    /// <summary>
    ///     ensures that ReportGenerator is called with the appropriate settings given the current state.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected ReportGeneratorSettings WithTag(ReportGeneratorSettings settings)
    {
        settings = settings.SetProcessToolPath(
            ToolPathResolver.GetPackageExecutable(
                "ReportGenerator",
                "ReportGenerator.dll",
                framework: "netcoreapp3.0"
            )
        );

        return this switch
        {
            IHaveGitVersion gitVersion => settings.SetTag(gitVersion.GitVersion?.InformationalVersion),
            IHaveGitRepository { GitRepository: { } } gitRepository => settings.SetTag(
                gitRepository.GitRepository.Head
            ),
            _ => settings
        };
    }
}

using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;
using static Nuke.Common.IO.FileSystemTasks;

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
    public IEnumerable<AbsolutePath> InputReports => CoverageDirectory
       .GlobFiles("**/*.cobertura.xml");

    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    public Target TriggerCodeCoverageReports => d => d
                                                    .TriggeredBy(Test)
                                                    .Unlisted()
                                                    .After(Test)
                                                    .Description("Generates code coverage reports")
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any());


    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    public Target GenerateCodeCoverageReportCobertura => d => d
                                                             .TriggeredBy(TriggerCodeCoverageReports)
                                                             .Unlisted()
                                                             .OnlyWhenDynamic(() => InputReports.Any())
                                                             .Executes(
                                                                  () =>
                                                                  {
                                                                      if (this is IHaveTestArtifacts { TestResultsDirectory: { } testResultsDirectory })
                                                                      {
                                                                          // Ensure anything that has been dropped in the test results from a collector is
                                                                          // into the coverage directory
                                                                          foreach (var file in testResultsDirectory
                                                                                              .GlobFiles("**/*.cobertura.xml")
                                                                                              .Where(x => Guid.TryParse(Path.GetFileName(x.Parent), out var _))
                                                                                              .SelectMany(coverage => coverage.Parent.GlobFiles("*.*")))
                                                                          {
                                                                              var folderName = Path.GetFileName(file.Parent);
                                                                              var extensionPart = string.Join(".", Path.GetFileName(file).Split('.').Skip(1));
                                                                              CopyFile(
                                                                                  file,
                                                                                  CoverageDirectory / $"{folderName}.{extensionPart}",
                                                                                  FileExistsPolicy.OverwriteIfNewer
                                                                              );
                                                                          }
                                                                      }
                                                                  }
                                                              )
                                                             .Executes(
                                                                  () =>
                                                                  {
                                                                      // var toolPath = ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
                                                                      ReportGeneratorTasks.ReportGenerator(
                                                                          s => WithTag(s)
                                                                              // .SetToolPath(toolPath)
                                                                              .SetFramework(
                                                                                   Constants.ReportGeneratorFramework
                                                                               )
                                                                              .SetReports(InputReports)
                                                                              .SetTargetDirectory(CoverageDirectory)
                                                                              .SetReportTypes(ReportTypes.Cobertura)
                                                                      );

                                                                      CopyFile(
                                                                          CoverageDirectory / "Cobertura.xml",
                                                                          CoverageDirectory / "solution.cobertura",
                                                                          FileExistsPolicy.OverwriteIfNewer
                                                                      );
                                                                      CopyFile(
                                                                          CoverageDirectory / "Cobertura.xml",
                                                                          CoverageDirectory / "solution.xml",
                                                                          FileExistsPolicy.OverwriteIfNewer
                                                                      );
                                                                      RenameFile(
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
        return ( this switch
        {
            IHaveGitVersion gitVersion => settings.SetTag(gitVersion.GitVersion.InformationalVersion),
            IHaveGitRepository { GitRepository: { } } gitRepository => settings.SetTag(
                gitRepository.GitRepository.Head
            ),
            _ => settings
        } ).SetFramework(Constants.ReportGeneratorFramework);
    }
}

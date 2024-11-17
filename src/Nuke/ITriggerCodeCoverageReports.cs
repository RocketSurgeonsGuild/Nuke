using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Triggers code coverage to happen
/// </summary>
/// <remarks>
///     This causes code coverage to trigger
/// </remarks>
[PublicAPI]
public interface ITriggerCodeCoverageReports : IHaveCodeCoverage, IHaveTestTarget, IHaveTestArtifacts, ITrigger
{
    /// <summary>
    ///     The input reports
    /// </summary>
    /// <remarks>
    ///     used to determine if any coverage was emitted, if not the tasks will skip to avoid errors
    /// </remarks>
    public IEnumerable<AbsolutePath> InputReports => CoverageDirectory.GlobFiles("**/*.cobertura.xml");

    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    [NonEntryTarget]
    public Target CollectCodeCoverage => d => d
                                             .TriggeredBy(Test)
                                             .After(Test)
                                             .Description("Collects code coverage results")
                                             .AssuredAfterFailure()
                                             .Executes(
                                                  () =>
                                                  {
                                                      // Ensure anything that has been dropped in the test results from a collector is
                                                      // into the coverage directory
                                                      foreach (var file in TestResultsDirectory.GlobFiles("**/*.cobertura.xml"))
//                                                                                      .Where(x => Guid.TryParse(Path.GetFileName(x.Parent), out _))
                                                      {
                                                          var relativePath = TestResultsDirectory.GetRelativePathTo(file);
                                                          file.Copy(CoverageDirectory.CreateOrCleanDirectory() / relativePath, ExistsPolicy.FileOverwriteIfNewer);
                                                      }
                                                  }
                                              );


    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    [NonEntryTarget]
    public Target GenerateCodeCoverageReportCobertura => d => d
                                                             .TriggeredBy(CollectCodeCoverage)
                                                             .Unlisted()
                                                             .AssuredAfterFailure()
                                                             .OnlyWhenDynamic(() => InputReports.Any())
                                                             .Executes(
                                                                  () =>
                                                                  {
                                                                      // var toolPath = ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
                                                                      ReportGeneratorTasks.ReportGenerator(
                                                                          s => Defaults(s)
                                                                              .SetTargetDirectory(CoverageDirectory)
                                                                              .SetReportTypes(ReportTypes.Cobertura)
                                                                      );

                                                                      ( CoverageDirectory / "Cobertura.xml" ).Copy(
                                                                          CoverageDirectory / "solution.cobertura",
                                                                          ExistsPolicy.FileOverwriteIfNewer
                                                                      );
                                                                      ( CoverageDirectory / "Cobertura.xml" ).Copy(
                                                                          CoverageDirectory / "solution.xml",
                                                                          ExistsPolicy.FileOverwriteIfNewer
                                                                      );
                                                                  }
                                                              );

    /// <summary>
    ///     ensures that ReportGenerator is called with the appropriate settings given the current state.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected ReportGeneratorSettings Defaults(ReportGeneratorSettings settings)
    {
        return ( this switch
                 {
                     IHaveGitVersion gitVersion                              => settings.SetTag(gitVersion.GitVersion.InformationalVersion),
                     IHaveGitRepository { GitRepository: { } } gitRepository => settings.SetTag(gitRepository.GitRepository.Head),
                     _                                                       => settings,
                 }
               )
              .SetReports(InputReports)
              .SetSourceDirectories(NukeBuild.RootDirectory)
              .SetFramework(Constants.ReportGeneratorFramework)
            ;
    }
}

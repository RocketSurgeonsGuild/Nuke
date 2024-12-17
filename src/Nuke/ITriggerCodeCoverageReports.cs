using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.ReportGenerator;
using Rocket.Surgery.Nuke.ProjectModel;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Triggers code coverage to happen
/// </summary>
/// <remarks>
///     This causes code coverage to trigger
/// </remarks>
[PublicAPI]
public interface ITriggerCodeCoverageReports : IHaveCodeCoverage, IHaveTestTarget, IHaveTestArtifacts, ITrigger, IHaveSolution
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
                                                      var files = TestResultsDirectory.GlobFiles("**/*.xml", "**/*.json", "**/*.coverage");
                                                      if (!files.Any())
                                                      {
                                                          return;
                                                      }

                                                      _ = ReportGeneratorTasks.ReportGenerator(
                                                          settings => settings
                                                                     .SetReports(files)
                                                                     .SetSourceDirectories(NukeBuild.RootDirectory)
                                                                     .SetProcessWorkingDirectory(RootDirectory)
                                                                     .SetTargetDirectory(CoverageDirectory)
                                                                     .AddReportTypes(
                                                                          ReportTypes.Cobertura,
                                                                          ReportTypes.Xml,
                                                                          ReportTypes.lcov,
                                                                          ReportTypes.Latex,
                                                                          ReportTypes.OpenCover
                                                                      )
                                                      );

                                                      _ = ( CoverageDirectory / "Cobertura.xml" ).Move(
                                                          CoverageDirectory / "test.cobertura.xml",
                                                          ExistsPolicy.FileOverwriteIfNewer
                                                      );
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
                                                             .ProceedAfterFailure()
                                                             .OnlyWhenDynamic(() => InputReports.Any())
                                                             .Executes(
                                                                  () =>
                                                                  {
                                                                      // var toolPath = ToolPathResolver.GetPackageExecutable("ReportGenerator", "ReportGenerator.dll", framework: "netcoreapp3.0");
                                                                      _ = ReportGeneratorTasks.ReportGenerator(
                                                                          s => Defaults(s)
                                                                              .SetTargetDirectory(CoverageDirectory)
                                                                              .SetReportTypes(ReportTypes.Cobertura)
                                                                      );
                                                                  }
                                                              );

    /// <summary>
    ///     ensures that ReportGenerator is called with the appropriate settings given the current state.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected ReportGeneratorSettings Defaults(ReportGeneratorSettings settings)
        => ( this switch
        {
            IHaveGitVersion gitVersion => settings.SetTag(gitVersion.GitVersion.InformationalVersion),
            IHaveGitRepository { GitRepository: { } } gitRepository => settings.SetTag(gitRepository.GitRepository.Head),
            _ => settings,
        }
           )
          .SetReports(InputReports)
          .SetSourceDirectories(NukeBuild.RootDirectory)
          .SetFramework(Constants.ReportGeneratorFramework)
          // this is more or less a hack / compromise because
          // I was unable to coverage to exclude everything in a given assembly by default.
          .AddAssemblyFilters(
               Solution
                  .AnalyzeAllProjects()
                  .Select(z => z.GetProperty("AssemblyName") ?? "")
                  .Where(z => !string.IsNullOrWhiteSpace(z))
                  .Distinct()
                  .Select(z => "+" + z)
           )
          .AddAssemblyFilters(
               Solution
                  .AnalyzeAllProjects()
                  .SelectMany(z => z.PackageReferences)
                  .Select(z => z.Name)
                  .Where(z => !string.IsNullOrWhiteSpace(z))
                  .Distinct()
                  .Select(z => "-" + z)
           );
}

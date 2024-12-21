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
    public IEnumerable<AbsolutePath> InputReports => TestResultsDirectory.GlobFiles("**/*.xml", "**/*.json", "**/*.coverage");

    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    [NonEntryTarget]
    public Target CollectCodeCoverage => d => d
                                             .TriggeredBy(Test)
                                             .After(Test)
                                             .Description("Collects code coverage results")
                                             .OnlyWhenDynamic(() => InputReports.Any())
                                             .AssuredAfterFailure()
                                             .Executes(
                                                  () =>
                                                  {
                                                      _ = ReportGeneratorTasks.ReportGenerator(
                                                          settings => Defaults(settings)
                                                                     .SetTargetDirectory(CoverageDirectory.CreateOrCleanDirectory())
                                                                     .AddReportTypes(
                                                                          ReportTypes.Cobertura,
                                                                          ReportTypes.Latex,
                                                                          ReportTypes.Clover
                                                                      )
                                                      );
                                                  }
                                              );


    /// <summary>
    ///     This will generate code coverage reports from emitted coverage data
    /// </summary>
    [NonEntryTarget]
    public Target GenerateCodeCoverageReportCobertura => d => d.DependsOn(CollectCodeCoverage);

    /// <summary>
    ///     ensures that ReportGenerator is called with the appropriate settings given the current state.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected ReportGeneratorSettings Defaults(ReportGeneratorSettings settings)
        => ( this switch
             {
                 IHaveGitVersion gitVersion                              => settings.SetTag(gitVersion.GitVersion.InformationalVersion),
                 IHaveGitRepository { GitRepository: { } } gitRepository => settings.SetTag(gitRepository.GitRepository.Head),
                 _                                                       => settings,
             }
           )
          .SetTargetDirectory(CoverageDirectory)
          .SetReports(InputReports)
          .SetSourceDirectories(NukeBuild.RootDirectory)
          .SetProcessWorkingDirectory(RootDirectory)
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

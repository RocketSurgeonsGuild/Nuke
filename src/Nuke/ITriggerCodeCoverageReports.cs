using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
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
                                                      ReportGeneratorTasks.ReportGenerator(
                                                          settings => settings
                                                                     .SetReports(TestResultsDirectory.GlobFiles("**/*.xml", "**/*.json", "**/*.coverage"))
                                                                     .SetSourceDirectories(NukeBuild.RootDirectory)
                                                                     .SetProcessWorkingDirectory(RootDirectory)
                                                                     .SetTargetDirectory(TemporaryDirectory)
                                                                     .SetReportTypes(ReportTypes.Cobertura)
                                                      );

                                                      ( TemporaryDirectory / "Cobertura.xml" ).Move(
                                                          CoverageDirectory / "solution.cobertura.xml",
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
                                                                      ReportGeneratorTasks.ReportGenerator(
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

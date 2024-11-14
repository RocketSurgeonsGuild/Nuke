using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.MsBuild;

/// <summary>
///     Defines a test task using msbuild
/// </summary>
public interface ICanTestWithXUnit : IHaveTestTarget,
    IHaveBuildTarget,
    IHaveTestArtifacts,
    IHaveSolution,
    IHaveConfiguration,
    IHaveGitVersion,
    IHaveOutputLogs,
    ICan
{
    /// <summary>
    ///     xunit test
    /// </summary>
    public Target NetTest => d => d
                                 .DependsOn(Build)
                                 .Unlisted()
                                 .Executes(
                                      async () =>
                                      {
                                          foreach (var project in Solution.GetTestProjects())
                                          {
                                              DotNetTasks
                                                 .DotNetTest(
                                                      settings =>
                                                          settings
                                                             .SetProjectFile(project.FilePath)
                                                             .SetConfiguration(Configuration)
                                                             .SetGitVersionEnvironment(GitVersion)
                                                             .SetDefaultLoggers(LogsDirectory / "test.log")
                                                             .EnableNoRestore()
                                                             .SetLoggers("trx")
                                                             .SetProperty("VSTestResultsDirectory", TestResultsDirectory)
                                                  );
                                          }
                                      }
                                  );
}

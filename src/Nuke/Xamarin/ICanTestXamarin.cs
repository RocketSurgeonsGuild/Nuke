using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Xamarin test run
    /// </summary>
    public interface ICanTestXamarin : IHaveTestTarget,
                                       IHaveBuildTarget,
                                       IHaveTestArtifacts,
                                       IComprehendTests,
                                       IHaveCodeCoverage,
                                       IHaveOutputLogs,
                                       IHaveConfiguration,
                                       IHaveGitVersion,
                                       IHaveSolution,
                                       ICan
    {
        /// <summary>
        /// test
        /// </summary>
        public new Target Test => _ => _
           .DependsOn(Build)
           .OnlyWhenStatic(() => FileSystemTasks.DirectoryExists(TestsDirectory))
           .Executes(
                () =>
                {
                    DotNetTasks.DotNetTest(
                        settings =>
                            settings.SetProjectFile(Solution)
                               .SetDefaultLoggers(LogsDirectory / "test.log")
                               .SetGitVersionEnvironment(GitVersion)
                               .SetConfiguration(Configuration)
                               .EnableNoRestore()
                               .SetLoggers("trx")
                               .SetProperty("CollectCoverage", "true")
                               .SetProperty(
                                    "DeterministicSourcePaths",
                                    "false"
                                ) // DeterministicSourcePaths being true breaks coverlet!
                               .SetProperty("CoverageDirectory", CoverageDirectory)
                               .SetResultsDirectory(TestResultsDirectory)
                    );

                    foreach (var coverage in TestResultsDirectory.GlobFiles("**/*.cobertura.xml"))
                    {
                        FileSystemTasks.CopyFileToDirectory(
                            coverage,
                            CoverageDirectory,
                            FileExistsPolicy.OverwriteIfNewer
                        );
                    }
                }
            );
    }
}
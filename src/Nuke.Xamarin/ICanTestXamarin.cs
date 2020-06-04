using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface ICanTestXamarin : IHaveTestTarget, IHaveBuildTarget, IHaveTestArtifacts, IComprehendTests, IHaveCodeCoverage, IHaveOutputLogs, IHaveConfiguration, IHaveGitVersion, IHaveSolution
    {
        
        /// <summary>
        /// test
        /// </summary>
        public new Target Test => _ => _
           .DependsOn(Build)
           .OnlyWhenStatic(() => FileSystemTasks.DirectoryExists(TestsDirectory))
           .Executes(() =>
            {
                DotNetTasks.DotNetTest(settings =>
                    DotNetTestSettingsExtensions.SetProjectFile<DotNetTestSettings>(settings, (string)Solution)
                       .SetDefaultLoggers(LogsDirectory / "test.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetConfiguration(Configuration)
                       .EnableNoRestore()
                       .SetLogger($"trx")
                       .SetProperty("CollectCoverage", "true")
                       .SetProperty("DeterministicSourcePaths", "false") // DeterministicSourcePaths being true breaks coverlet!
                       .SetProperty("CoverageDirectory", CoverageDirectory)
                       .SetResultsDirectory(TestResultsDirectory));

                foreach (var coverage in TestResultsDirectory.GlobFiles("**/*.cobertura.xml"))
                {
                    FileSystemTasks.CopyFileToDirectory(coverage, CoverageDirectory, FileExistsPolicy.OverwriteIfNewer);
                }
            });
    }
}
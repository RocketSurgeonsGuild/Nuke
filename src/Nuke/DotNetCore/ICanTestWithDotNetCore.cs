using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using System;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Defines a `dotnet test` test run with code coverage via coverlet
    /// </summary>
    public interface ICanTestWithDotNetCore : IHaveCollectCoverage,
                                              IHaveBuildTarget,
                                              ITriggerCodeCoverageReports,
                                              IComprehendTests,
                                              IHaveTestArtifacts,
                                              IHaveGitVersion,
                                              IHaveSolution,
                                              IHaveConfiguration,
                                              IHaveOutputLogs,
                                              ICan
    {
        /// <summary>
        /// dotnet test
        /// </summary>
        public Target CoreTest => _ => _
           .Description("Executes all the unit tests.")
           .After(Build)
           .OnlyWhenDynamic(() => TestsDirectory.GlobFiles("**/*.csproj").Count > 0)
           .WhenSkipped(DependencyBehavior.Execute)
           .Executes(
                () => DotNetTasks.DotNetBuild(
                    s => s
                       .SetProjectFile(Solution)
                       .SetDefaultLoggers(LogsDirectory / "test.build.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetConfiguration("Debug")
                       .EnableNoRestore()
                )
            )
           .Executes(
                () =>
                {
                    EnsureCleanDirectory(TestResultsDirectory);
                    CoverageDirectory.GlobFiles("*.cobertura.xml", "*.opencover.xml", "*.json", "*.info")
                       .Where(x => Guid.TryParse(Path.GetFileName(x)?.Split('.')[0], out var _))
                       .ForEach(DeleteFile);
                }
            )
           .Executes(
                async () =>
                {
                    var runsettings = TestsDirectory / "coverlet.runsettings";
                    if (!FileExists(runsettings))
                    {
                        runsettings = NukeBuild.TemporaryDirectory / "default.runsettings";
                        using var tempFile = File.Open(runsettings, FileMode.OpenOrCreate);
                        await typeof(ICanTestWithDotNetCore).Assembly
                           .GetManifestResourceStream("Rocket.Surgery.Nuke.default.runsettings")!
                           .CopyToAsync(tempFile)
                           .ConfigureAwait(false);
                    }

                    DotNetTasks.DotNetTest(
                        s => s
                           .SetProjectFile(Solution)
                           .SetDefaultLoggers(LogsDirectory / "test.log")
                           .SetGitVersionEnvironment(GitVersion)
                           .SetConfiguration("Debug")
                           .EnableNoRestore()
                           .EnableNoBuild()
                           .SetLoggers("trx")
                            // DeterministicSourcePaths being true breaks coverlet!
                           .SetProperty("DeterministicSourcePaths", "false")
                           .SetResultsDirectory(TestResultsDirectory)
                           .When(
                                !CollectCoverage,
                                x => x.SetProperty((string)"CollectCoverage", "true")
                                   .SetProperty("CoverageDirectory", CoverageDirectory)
                            )
                           .When(
                                CollectCoverage,
                                x => x
                                   .SetProperty("CollectCoverage", "false")
                                   .SetDataCollector("XPlat Code Coverage")
                                   .SetSettingsFile(runsettings)
                            )
                    );

                    // Ensure anything that has been dropped in the test results from a collector is
                    // into the coverage directory
                    foreach (var file in TestResultsDirectory
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
            );
    }
}
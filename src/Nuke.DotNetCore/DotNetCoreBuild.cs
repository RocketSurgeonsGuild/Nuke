using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    public interface IRestoreWithDotNetCore : IHaveCleanTarget,
                                              IHaveSolution,
                                              IOutputLogs,
                                              IHaveGitVersion,
                                              IHaveRestoreTarget
    {
        /// <summary>
        /// This will ensure that all local dotnet tools are installed
        /// </summary>
        public Target DotnetToolRestore => _ => _
           .After(Clean)
           .OnlyWhenStatic(() => FileExists(NukeBuild.RootDirectory / ".config/dotnet-tools.json"))
           .Unlisted()
           .Executes(() => DotNet("tool restore"));

        /// <summary>
        /// dotnet restore
        /// </summary>
        public Target CoreRestore => _ => _
           .Description("Restores the dependencies.")
           .Unlisted()
           .After(Clean)
           .DependentFor(CoreRestore)
           .DependsOn(DotnetToolRestore)
           .Executes(
                () =>
                {
                    DotNetRestore(
                        s => s
                           .SetProjectFile(Solution)
                           .SetDisableParallel(true)
                           .SetDefaultLoggers(LogsDirectory / "restore.log")
                           .SetGitVersionEnvironment(GitVersion)
                    );
                }
            );
    }

    public interface IBuildWithDotNetCore : IHaveRestoreTarget,
                                            IHaveConfiguration,
                                            IHaveBuildTarget,
                                            IHaveSolution,
                                            IOutputLogs,
                                            IHaveGitVersion
    {
        /// <summary>
        /// dotnet build
        /// </summary>
        public Target CoreBuild => _ => _
           .Description("Builds all the projects.")
           .DependsOn(Restore)
           .DependentFor(CoreBuild)
           .Executes(
                () =>
                {
                    DotNetBuild(
                        s => s
                           .SetProjectFile(Solution)
                           .SetDefaultLoggers(LogsDirectory / "build.log")
                           .SetGitVersionEnvironment(GitVersion)
                           .SetConfiguration(Configuration)
                           .EnableNoRestore()
                    );
                }
            );
    }

    public interface IUseDataCollector : IHaveCollectCoverage
    {
        bool IHaveCollectCoverage.CollectCoverage => true;
    }

    public interface IUseMsBuildCoverage : IHaveCollectCoverage
    {
        bool IHaveCollectCoverage.CollectCoverage => false;
    }

    public interface IHaveCollectCoverage
    {
        bool CollectCoverage { get; }
    }

    public interface ITestWithDotNetCore : IHaveCollectCoverage,
                                           IHaveBuildTarget,
                                           ITriggerCodeCoverageReports,
                                           IIncludeTests,
                                           IOutputTestArtifacts,
                                           IHaveGitVersion,
                                           IHaveSolution,
                                           IHaveConfiguration,
                                           IOutputLogs
    {
        /// <summary>
        /// dotnet test
        /// </summary>
        public Target CoreTest => _ => _
           .Description("Executes all the unit tests.")
           .DependentFor(CoreTest)
           .After(Build)
           .OnlyWhenStatic(() => DirectoryExists(TestDirectory))
           .OnlyWhenDynamic(() => TestDirectory.GlobFiles("**/*.csproj").Count > 0)
           .WhenSkipped(DependencyBehavior.Execute)
           .Executes(
                () =>
                {
                    EnsureCleanDirectory(TestResultsDirectory);
                    CoverageDirectory.GlobFiles("*.cobertura.xml", "*.opencover.xml", "*.json", "*.info")
                       .Where(x => Guid.TryParse(Path.GetFileName(x).Split('.')[0], out var _))
                       .ForEach(DeleteFile);
                },
                async () =>
                {
                    var runsettings = TestDirectory / "coverlet.runsettings";
                    if (!FileExists(runsettings))
                    {
                        runsettings = NukeBuild.TemporaryDirectory / "default.runsettings";
                        if (!FileExists(runsettings))
                        {
                            using var tempFile = File.Open(runsettings, FileMode.CreateNew);
                            await typeof(ITestWithDotNetCore).Assembly
                                   .GetManifestResourceStream("Rocket.Surgery.Nuke.DotNetCore.default.runsettings")!
                               .CopyToAsync(tempFile)
                               .ConfigureAwait(false);
                        }
                    }

                    DotNetTest(
                        s => s
                           .SetProjectFile(Solution)
                           .SetDefaultLoggers(LogsDirectory / "test.log")
                           .SetGitVersionEnvironment(GitVersion)
                           .SetConfiguration("Debug")
                           .EnableNoRestore()
                           .SetLogger("trx")
                            // DeterministicSourcePaths being true breaks coverlet!
                           .SetProperty("DeterministicSourcePaths", "false")
                           .SetResultsDirectory(TestResultsDirectory)
                           .When(
                                !CollectCoverage,
                                x => x
                                   .SetProperty("CollectCoverage", "true")
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

    public interface IPackWithDotNetCore : IHaveBuildTarget,
                                           IOutputNuGetArtifacts,
                                           IHaveTestTarget,
                                           IHavePackTarget,
                                           IHaveSolution,
                                           IOutputLogs,
                                           IHaveGitVersion,
                                           IHaveConfiguration
    {
        /// <summary>
        /// dotnet pack
        /// </summary>
        public Target CorePack => _ => _
           .Description("Packs all the NuGet packages.")
           .DependsOn(Build)
           .DependentFor(CorePack)
           .After(Test)
           .Executes(
                () => DotNetPack(
                    s => s
                       .SetProject(Solution)
                       .SetDefaultLoggers(LogsDirectory / "pack.log")
                       .SetGitVersionEnvironment(GitVersion)
                       .SetConfiguration(Configuration)
                       .EnableNoRestore()
                       .EnableNoBuild()
                       .SetOutputDirectory(NuGetPackageDirectory)
                )
            );
    }
}
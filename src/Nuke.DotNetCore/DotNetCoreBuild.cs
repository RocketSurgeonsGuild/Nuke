using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Tools.VSWhere;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using System.IO;
using System.Linq;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Base build plan for .NET Core based applications
    /// </summary>
    public abstract class DotNetCoreBuild : RocketBoosterBuild
    {
        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        public Target DotNetCore => _ => _;

        /// <summary>
        /// Use Coverage Collector instead of msbuild collector for code coverage
        /// </summary>
        [Parameter("Use Coverage Collector instead of msbuild collector for code coverage")]
        public readonly bool CoverageCollector = false;

        // /// <summary>
        // /// This will ensure that all local dotnet tools are installed
        // /// </summary>
        // public Target DotnetToolRestore => _ => _
        //    .DependsOn(Clean)
        //    .DependentFor(DotNetCore)
        //    .Unlisted()
        //    .Executes(() => DotNet("tool restore"));

        /// <summary>
        /// dotnet restore
        /// </summary>
        public Target Restore => _ => _
            .DependentFor(DotNetCore)
            // .DependsOn(DotnetToolRestore)
            .DependsOn(Clean)
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution)
                    .SetDisableParallel(true)
                    .SetBinaryLogger(LogsDirectory / "restore.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "restore.log")
                    .SetGitVersionEnvironment(GitVersion)
                );
            });

        /// <summary>
        /// dotnet build
        /// </summary>
        public Target Build => _ => _
            .DependsOn(Restore)
            .DependentFor(DotNetCore)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetBinaryLogger(LogsDirectory / "build.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "build.log")
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            });

        /// <summary>
        /// dotnet test
        /// </summary>
        public Target Test => _ => _
            .DependsOn(Build)
            .DependentFor(DotNetCore)
            .DependentFor(Pack)
            .DependentFor(Generate_Code_Coverage_Reports)
            .Triggers(Generate_Code_Coverage_Reports)
            .OnlyWhenDynamic(() => TestDirectory.GlobFiles("**/*.csproj").Count > 0)
            .WhenSkipped(DependencyBehavior.Execute)
            .Executes( async () =>
            {
                DotNetTest(s => {
                    var a = s
                        .SetProjectFile(Solution)
                        .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                        .SetFileLogger(LogsDirectory / "test.log")
                        .SetGitVersionEnvironment(GitVersion)
                        .SetConfiguration("Debug")
                        .EnableNoRestore()
                        // .EnableNoBuild()
                        .SetLogger($"trx")
                        .SetProperty("CollectCoverage", !CoverageCollector)
                        .SetProperty("CollectCoverage", "true")
                        // DeterministicSourcePaths being true breaks coverlet!
                        .SetProperty("DeterministicSourcePaths", "false")
                        .SetProperty("CoverageDirectory", CoverageDirectory)
                        .SetProperty("IncludeDirectory", string.Join(";", TestDirectory.GlobDirectories("**/bin/*/*").Select(x => (string)x).Take(1)))
                        .SetResultsDirectory(TestResultsDirectory);
                    var b = (FileExists(TestDirectory / "coverlet.runsettings") ? a.SetSettingsFile(TestDirectory / "coverlet.runsettings") : a);
                    return CoverageCollector ? b.SetDataCollector("XPlat Code Coverage") : b;
                });

                /// TEMP
                foreach (var item in TestDirectory.GlobDirectories("**/bin/*/*"))
                {
                    using (var outFile = File.OpenWrite(CoverageDirectory / (((string)item).Replace("\\", "-").Replace("/", "-").Replace(":", "-") + ".dir")))
                    using (var writer = new StreamWriter(outFile))
                    {
                        foreach (var file in Directory.EnumerateFiles(item))
                        {
                           await writer.WriteLineAsync(file);
                        }
                    }
                }
                foreach (var coverage in TestResultsDirectory.GlobFiles("**/*.cobertura.xml"))
                {
                    CopyFileToDirectory(coverage, CoverageDirectory, FileExistsPolicy.OverwriteIfNewer);
                }
            });

        /// <summary>
        /// dotnet pack
        /// </summary>
        public Target Pack => _ => _
            .DependsOn(Build)
            .DependentFor(DotNetCore)
            .Executes(() =>
            {
                DotNetPack(s => s
                    .SetProject(Solution)
                    .SetBinaryLogger(LogsDirectory / "pack.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "pack.log")
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .EnableNoBuild()
                    .SetOutputDirectory(NuGetPackageDirectory));
            });
    }
}

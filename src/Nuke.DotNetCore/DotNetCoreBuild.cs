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
                    .SetFileLogger(LogsDirectory / "restore.log", Verbosity)
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
                    .SetFileLogger(LogsDirectory / "build.log", Verbosity)
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
            .Executes(() =>
            {
                DotNetTest(s =>
                {
                    var a = s
                        .SetProjectFile(Solution)
                        .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                        .SetFileLogger(LogsDirectory / "test.log", Verbosity)
                        .SetGitVersionEnvironment(GitVersion)
                        .SetConfiguration(Configuration)
                        .EnableNoRestore()
                        .SetLogger($"trx")
                        .SetProperty("CollectCoverage", (IsLocalBuild && !Force))
                        .SetProperty("CoverageDirectory", CoverageDirectory)
                        .SetResultsDirectory(TestResultsDirectory);
                    var b = (FileExists(TestDirectory / "coverlet.runsettings") ? a.SetSettingsFile(TestDirectory / "coverlet.runsettings") : a);
                    return !IsLocalBuild || Force ? b.SetDataCollector("XPlat Code Coverage") : b;
                });
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
                    .SetFileLogger(LogsDirectory / "pack.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetOutputDirectory(NuGetPackageDirectory));
            });
    }
}

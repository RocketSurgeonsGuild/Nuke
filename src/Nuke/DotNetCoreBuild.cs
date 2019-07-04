using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Tools.VSWhere;

namespace Rocket.Surgery.Nuke
{
    public abstract class DotNetCoreBuild : RocketBoosterBuild
    {
        public Target Core => _ => _;

        public Target InstallTools => _ => _
            .DependsOn(Clean)
            .DependentFor(Core)
            .Unlisted()
            .Executes(() => DotNet("tool restore"));

        public Target Restore => _ => _
            .DependentFor(Core)
            .DependsOn(InstallTools)
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

        public Target Build => _ => _
            .DependsOn(Restore)
            .DependentFor(Core)
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

        public Target Test => _ => _
            .DependsOn(Build)
            .DependentFor(Core)
            .DependentFor(Pack)
            .Triggers(Generate_Code_Coverage_Reports)
            .OnlyWhenDynamic(() => TestDirectory.GlobFiles("test/**/*.csproj").Count > 0)
            .WhenSkipped(DependencyBehavior.Execute)
            .Executes(() =>
            {
            // TestDirectory.GlobFiles("**/*.csproj")
            //     .ForEach((Project) =>
            //     {
            //         var name = Path.GetFileNameWithoutExtension(Project).ToLowerInvariant();
            //         // var name = Project.
            //         DotNetTest(s => s
            //             .SetProjectFile(Project)
            //             .SetBinaryLogger(LogsDirectory / $"{name}.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
            //             .SetFileLogger(LogsDirectory / $"{name}.log", Verbosity)
            //             .SetGitVersionEnvironment(GitVersion)
            //             .SetConfiguration(Configuration)
            //             .EnableNoRestore()
            //             .SetLogger($"trx;LogFileName={TestResultsDirectory / $"{name}.trx"}")
            //             .SetProperty("CollectCoverage", true)
            //             .SetProperty("CoverageDirectory", CoverageDirectory)
            //             .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
            //     });

            DotNetTest(s => s
                    .SetProjectFile(Solution)
                    .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                    .SetFileLogger(LogsDirectory / "test.log", Verbosity)
                    .SetGitVersionEnvironment(GitVersion)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore()
                    .SetLogger($"trx")
                    .SetProperty("CollectCoverage", true)
                    .SetProperty("CoverageDirectory", CoverageDirectory)
                    .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
            });

        public Target Pack => _ => _
            .DependsOn(Build)
            .DependentFor(Core)
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

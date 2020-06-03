using System;
using System.Linq.Expressions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface IHaveXamarinConfiguration : IHaveConfiguration
    {
        string IHaveConfiguration.Configuration => Configuration;
        
        new XamarinConfiguration Configuration { get; }
    }
    public interface IRestoreXamarin : IHaveGitVersion, IHaveSolution, IHaveCleanTarget, IHaveRestoreTarget
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        /// <remarks>https://developercommunity.visualstudio.com/content/problem/20550/cant-run-dotnet-restore.html</remarks>
        public Target Restore => _ => _
           .DependsOn(Clean)
           .Executes(() => NuGetRestore(settings =>
                settings
                   .SetTargetPath(Solution)
                   .SetGitVersionEnvironment(GitVersion)
                   .SetNoCache(true)));
    }

    public interface IBuildXamarin : IHaveRestoreTarget, IHaveSolution, IHaveConfiguration, IHaveGitVersion, IOutputLogs
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
           .DependsOn(Restore)
           .Executes(() => MSBuild(settings =>
                settings
                   .SetSolutionFile(Solution)
                   .SetTargetPlatform(MSBuildTargetPlatform.x64)
                   .SetConfiguration(Configuration)
                   .SetDefaultLoggers(LogsDirectory / "build.log")
                   .SetGitVersionEnvironment(GitVersion)
                   .SetAssemblyVersion(GitVersion.AssemblySemVer)
                   .SetPackageVersion(GitVersion.NuGetVersionV2)));
    }

    public interface ITestXamarin : IHaveTestTarget, IHaveBuildTarget, IOutputTestArtifacts, IIncludeTests, IIncludeCodeCoverage, IOutputLogs, IHaveConfiguration, IHaveGitVersion, IHaveSolution
    {
        
        /// <summary>
        /// test
        /// </summary>
        public Target Test => _ => _
           .DependsOn(Build)
           .OnlyWhenStatic(() => DirectoryExists(TestDirectory))
           .Executes(() =>
            {
                DotNetTest(settings =>
                    settings
                       .SetProjectFile(Solution)
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
                    CopyFileToDirectory(coverage, CoverageDirectory, FileExistsPolicy.OverwriteIfNewer);
                }
            });
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Rocket.Surgery.Nuke;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin based applications
    /// </summary>
    public abstract class XamarinBuild : RocketBoosterBuild
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        public virtual Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                DotNetTasks
                    .DotNetRestore(settings =>
                        settings
                            .SetProjectFile(Solution)
                            .SetDisableParallel(true)
                            .SetDefaultLoggers(LogsDirectory / "restore.log")
                            .SetGitVersionEnvironment(GitVersion));
            });

        /// <summary>
        /// msbuild
        /// </summary>
        public virtual Target Build => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                MSBuildTasks
                    .MSBuild(settings =>
                        settings
                            .SetSolutionFile(Solution)
                            .SetConfiguration(Configuration)
                            .SetDefaultLoggers(LogsDirectory / "build.log")
                            .SetGitVersionEnvironment(GitVersion)
                            .SetAssemblyVersion(GitVersion.AssemblySemVer)
                            .SetPackageVersion(GitVersion.NuGetVersionV2));
            });

        /// <summary>
        /// xunit test
        /// </summary>
        public virtual Target Test => _ => _
            .DependsOn(Build)
            .Executes(() =>
            {
                DotNetTasks
                    .DotNetTest(settings =>
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
                    FileSystemTasks.CopyFileToDirectory(coverage, CoverageDirectory, FileExistsPolicy.OverwriteIfNewer);
                }
            });

    }
}

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
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

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
        public static ITargetDefinition Restore(ITargetDefinition _, IXamarinBuild build) => _
            .DependsOn(build.Clean)
            .Executes(() =>
            {
                DotNetRestore(settings =>
                    settings
                        .SetProjectFile(build.Solution)
                        .SetDisableParallel(true)
                        .SetDefaultLoggers(build.LogsDirectory / "restore.log")
                        .SetGitVersionEnvironment(build.GitVersion));
            });

        /// <summary>
        /// msbuild
        /// </summary>
        public static ITargetDefinition Build(ITargetDefinition _, IXamarinBuild build) => _
            .DependsOn(build.Restore)
            .Executes(() =>
            {
                MSBuild(settings =>
                    settings
                        .SetSolutionFile(build.Solution)
                        .SetConfiguration(build.Configuration)
                        .SetDefaultLoggers(build.LogsDirectory / "build.log")
                        .SetGitVersionEnvironment(build.GitVersion)
                        .SetAssemblyVersion(build.GitVersion.AssemblySemVer)
                        .SetPackageVersion(build.GitVersion.NuGetVersionV2));
            });

        /// <summary>
        /// xunit test
        /// </summary>
        public static ITargetDefinition Test(ITargetDefinition _, IXamarinBuild build) => _
            .DependsOn(build.Build)
            .Executes(() =>
            {
                DotNetTest(settings =>
                    settings
                        .SetProjectFile(build.Solution)
                        .SetDefaultLoggers(build.LogsDirectory / "test.log")
                        .SetGitVersionEnvironment(build.GitVersion)
                        .SetConfiguration(build.Configuration)
                        .EnableNoRestore()
                        .SetLogger($"trx")
                        .SetProperty("CollectCoverage", "true")
                        .SetProperty("DeterministicSourcePaths", "false") // DeterministicSourcePaths being true breaks coverlet!
                        .SetProperty("CoverageDirectory", build.CoverageDirectory)
                        .SetResultsDirectory(build.TestResultsDirectory));

                foreach (var coverage in build.TestResultsDirectory.GlobFiles("**/*.cobertura.xml"))
                {
                    CopyFileToDirectory(coverage, build.CoverageDirectory, FileExistsPolicy.OverwriteIfNewer);
                }
            });

    }
}

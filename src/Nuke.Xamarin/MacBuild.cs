using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
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
    /// Base build plan for Xamarin.Mac based applications
    /// </summary>
    public abstract class MacBuild : RocketBoosterBuild
    {
        /// <inheritdoc />
        public Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                NuGetRestore(settings =>
                    settings
                        .SetSolutionDirectory(Solution)
                        .EnableNoCache());
            });


        /// <inheritdoc />
        public Target Build => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                MSBuild(settings =>
                    settings
                        .SetSolutionFile(Solution)
                        .SetConfiguration(Configuration)
                        .SetDefaultLoggers(LogsDirectory / "build.log")
                        .SetGitVersionEnvironment(GitVersion)
                        .SetAssemblyVersion(GitVersion.AssemblySemVer)
                        .SetPackageVersion(GitVersion.NuGetVersionV2)
                        .SetTargets("Publish")
                        .SetOutDir(OutputDirectory));
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

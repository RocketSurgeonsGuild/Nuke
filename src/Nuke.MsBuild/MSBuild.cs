using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.ProjectModel;
using Buildalyzer;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Base build plan for .NET Framework based applications
    /// </summary>
    public abstract class MsBuild : RocketBoosterBuild
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public AbsolutePath NuspecDirectory => RootDirectory / ".nuspec";

        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        public Target NetFramework => _ => _;

        /// <summary>
        /// nuget restore
        /// </summary>
        public Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                NuGetTasks
                    .NuGetRestore(settings =>
                        settings
                            .SetSolutionDirectory(Solution)
                            .EnableNoCache());
            });

        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
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
        public Target Test => _ => _
            .DependsOn(Build)
            .DependentFor(Pack)
            .DependentFor(NetFramework)
            .Executes(() =>
            {
                foreach (var project in Solution.GetTestProjects())
                {
                    DotNetTasks
                        .DotNetTest(settings =>
                            settings
                                .SetProjectFile(project)
                                .SetConfiguration(Configuration)
                                .SetGitVersionEnvironment(GitVersion)
                                .SetDefaultLoggers(LogsDirectory / "test.log")
                                .EnableNoRestore()
                                .SetLogger($"trx")
                                .SetProperty("VSTestResultsDirectory", TestResultsDirectory));
                }
            });

        /// <summary>
        /// nuget pack
        /// </summary>
        public Target Pack => _ => _
            .DependsOn(Build)
            .DependentFor(NetFramework)
            .Executes(() =>
            {
                foreach (var project in NuspecDirectory.GlobFiles("*.nuspec"))
                {
                    NuGetTasks
                        .NuGetPack(settings =>
                            settings
                                .SetTargetPath(project)
                                .SetConfiguration(Configuration)
                                .SetGitVersionEnvironment(GitVersion)
                                .SetVersion(GitVersion.NuGetVersionV2)
                                .SetOutputDirectory(NuGetPackageDirectory)
                                .SetSymbols(true));
                }
            });
    }
}

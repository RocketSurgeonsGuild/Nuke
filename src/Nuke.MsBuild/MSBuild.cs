using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.IO.PathConstruction;
#pragma warning disable CA1724

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Base build plan for .NET Framework based applications
    /// </summary>
    public abstract class MSBuild : RocketBoosterBuild
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public static AbsolutePath NuspecDirectory => RootDirectory / ".nuspec";

        /// <summary>
        /// nuget restore
        /// </summary>
        public static ITargetDefinition Restore(ITargetDefinition _, IMsBuild build) => _
           .DependsOn(build.Clean)
           .Executes(
                () =>
                {
                    NuGetTasks
                       .NuGetRestore(
                            settings =>
                                settings
                                   .SetSolutionDirectory(build.Solution)
                                   .EnableNoCache()
                        );
                }
            );

        /// <summary>
        /// msbuild
        /// </summary>
        public static ITargetDefinition Build(ITargetDefinition _, IMsBuild build) => _
           .DependsOn(build.Restore)
           .Executes(
                () =>
                {
                    MSBuildTasks
                       .MSBuild(
                            settings =>
                                settings
                                   .SetSolutionFile(build.Solution)
                                   .SetConfiguration(build.Configuration)
                                   .SetDefaultLoggers(build.LogsDirectory / "build.log")
                                   .SetGitVersionEnvironment(build.GitVersion)
                                   .SetAssemblyVersion(build.GitVersion.AssemblySemVer)
                                   .SetPackageVersion(build.GitVersion.NuGetVersionV2)
                        );
                }
            );

        /// <summary>
        /// xunit test
        /// </summary>
        public static ITargetDefinition Test(ITargetDefinition _, IMsBuild build) => _
           .DependsOn(build.Build)
           .DependentFor(build.Pack)
           .Executes(
                () =>
                {
                    foreach (var project in build.Solution.GetTestProjects())
                    {
                        DotNetTasks
                           .DotNetTest(
                                settings =>
                                    settings
                                       .SetProjectFile(project)
                                       .SetConfiguration(build.Configuration)
                                       .SetGitVersionEnvironment(build.GitVersion)
                                       .SetDefaultLoggers(build.LogsDirectory / "test.log")
                                       .EnableNoRestore()
                                       .SetLogger("trx")
                                       .SetProperty("VSTestResultsDirectory", build.TestResultsDirectory)
                            );
                    }
                }
            );

        /// <summary>
        /// nuget pack
        /// </summary>
        public static ITargetDefinition Pack(ITargetDefinition _, IMsBuild build) => _
           .DependsOn(build.Build)
           .Executes(
                () =>
                {
                    foreach (var project in build.NuspecDirectory.GlobFiles("*.nuspec"))
                    {
                        NuGetTasks
                           .NuGetPack(
                                settings =>
                                    settings
                                       .SetTargetPath(project)
                                       .SetConfiguration(build.Configuration)
                                       .SetGitVersionEnvironment(build.GitVersion)
                                       .SetVersion(build.GitVersion.NuGetVersionV2)
                                       .SetOutputDirectory(build.NuGetPackageDirectory)
                                       .SetSymbols(true)
                            );
                    }
                }
            );
    }
}
using System;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.IO.PathConstruction;
#pragma warning disable CA1724

namespace Rocket.Surgery.Nuke.MsBuild
{
    public interface IRestoreWithMSBuild : IHaveCleanTarget, IHaveSolution
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        public Target Restore => _ => _
           .DependsOn(Clean)
           .Executes(() => NuGetTasks
               .NuGetRestore(
                    settings =>
                        settings
                           .SetSolutionDirectory(Solution)
                           .EnableNoCache()
                )
            );
    }
    public interface ITestWithXUnit : IHaveBuildTarget, IOutputTestArtifacts, IHaveSolution, IHaveConfiguration, IHaveGitVersion, IOutputLogs
    {
        /// <summary>
        /// xunit test
        /// </summary>
        public Target Test => _ => _
           .DependsOn(Build)
           .Executes(
                () =>
                {
                    foreach (var project in Solution.GetTestProjects())
                    {
                        DotNetTasks
                           .DotNetTest(
                                settings =>
                                    settings
                                       .SetProjectFile(project)
                                       .SetConfiguration(Configuration)
                                       .SetGitVersionEnvironment(GitVersion)
                                       .SetDefaultLoggers(LogsDirectory / "test.log")
                                       .EnableNoRestore()
                                       .SetLogger("trx")
                                       .SetProperty("VSTestResultsDirectory", TestResultsDirectory)
                            );
                    }
                }
            );

        
    }
    public interface IBuildWithMSBuild : IHaveRestoreTarget, IHaveSolution, IHaveConfiguration, IOutputLogs, IHaveGitVersion
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target Build => _ => _
           .DependsOn(Restore)
           .Executes(
                () => MSBuildTasks.MSBuild(
                        settings =>
                            settings
                               .SetSolutionFile(Solution)
                               .SetConfiguration(Configuration)
                               .SetDefaultLoggers(LogsDirectory / "build.log")
                               .SetGitVersionEnvironment(GitVersion)
                               .SetAssemblyVersion(GitVersion.AssemblySemVer)
                               .SetPackageVersion(GitVersion.NuGetVersionV2)
                    )
            );
    }

    public interface IPackWithMSBuild : IHavePackTarget, IHaveBuildTarget, IHaveTestTarget, IOutputNuGetArtifacts, IHaveGitVersion, IHaveConfiguration
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public static AbsolutePath NuspecDirectory => NukeBuild.RootDirectory / ".nuspec";
        
        /// <summary>
        /// nuget pack
        /// </summary>
        public Target Pack => _ => _
           .DependsOn(Build)
           .After(Test)
           .Executes(
                () =>
                {
                    foreach (var project in NuspecDirectory.GlobFiles("*.nuspec"))
                    {
                        NuGetTasks
                           .NuGetPack(
                                settings =>
                                    settings
                                       .SetTargetPath(project)
                                       .SetConfiguration(Configuration)
                                       .SetGitVersionEnvironment(GitVersion)
                                       .SetVersion(GitVersion.NuGetVersionV2)
                                       .SetOutputDirectory(NuGetPackageDirectory)
                                       .SetSymbols(true)
                            );
                    }
                }
            );
        
    }
}
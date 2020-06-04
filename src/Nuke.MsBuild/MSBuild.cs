using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;

#pragma warning disable CA1724

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Defines a restore task using msbuild
    /// </summary>
    public interface IRestoreWithMSBuild : IHaveRestoreTarget, IHaveCleanTarget, IHaveSolution
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        public Target NetRestore => _ => _
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

    /// <summary>
    /// Defines a test task using msbuild
    /// </summary>
    public interface ITestWithXUnit : IHaveTestTarget, IHaveBuildTarget, IOutputTestArtifacts, IHaveSolution, IHaveConfiguration, IHaveGitVersion, IOutputLogs
    {
        /// <summary>
        /// xunit test
        /// </summary>
        public Target NetTest => _ => _
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

    /// <summary>
    /// Defines a build task using msbuild
    /// </summary>
    public interface IBuildWithMSBuild : IHaveBuildTarget, IHaveRestoreTarget, IHaveSolution, IHaveConfiguration, IOutputLogs, IHaveGitVersion
    {
        /// <summary>
        /// msbuild
        /// </summary>
        public Target NetBuild => _ => _
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

    /// <summary>
    /// Pack using msbuild
    /// </summary>
    public interface IPackWithMSBuild : IHavePackTarget, IHaveBuildTarget, IHaveTestTarget, IOutputNuGetArtifacts, IHaveGitVersion, IHaveConfiguration
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public static AbsolutePath NuspecDirectory => NukeBuild.RootDirectory / ".nuspec";

        /// <summary>
        /// nuget pack
        /// </summary>
        public Target NetPack => _ => _
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
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
    /// <summary>
    /// Base build plan for Xamarin based applications
    /// </summary>
    public abstract class XamarinBuild : RocketBoosterBuild<XamarinConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XamarinBuild"/> class.
        /// </summary>
        protected XamarinBuild()
            : base(() => IsLocalBuild ? XamarinConfiguration.DebugMock : XamarinConfiguration.Mock)
        {
        }

        /// <summary>
        /// A value indicated whether the build host is OSX.
        /// </summary>
        public Expression<Func<bool>> IsOsx { get; set; } = () => EnvironmentInfo.Platform == PlatformFamily.OSX;

        /// <summary>
        /// nuget restore
        /// </summary>
        /// <remarks>https://developercommunity.visualstudio.com/content/problem/20550/cant-run-dotnet-restore.html</remarks>
        public static ITargetDefinition Restore(ITargetDefinition _, IXamarinBuild build) => _
           .DependsOn(build.Clean)
           .Executes(() => NuGetRestore(settings =>
                                settings
                                   .SetTargetPath(build.Solution)
                                   .SetDisableParallelProcessing(true)
                                   .SetDefaultLoggers(build.LogsDirectory / "restore.log")
                                   .SetGitVersionEnvironment(build.GitVersion)));

        /// <summary>
        /// msbuild
        /// </summary>
        public static ITargetDefinition Build(ITargetDefinition _, IXamarinBuild build) => _
            .DependsOn(build.Restore)
            .Executes(() => MSBuild(settings =>
                                settings
                                    .SetSolutionFile(build.Solution)
                                    .SetTargetPlatform(MSBuildTargetPlatform.x64)
                                    .SetConfiguration(build.Configuration)
                                    .SetDefaultLoggers(build.LogsDirectory / "build.log")
                                    .SetGitVersionEnvironment(build.GitVersion)
                                    .SetAssemblyVersion(build.GitVersion.AssemblySemVer)
                                    .SetPackageVersion(build.GitVersion.NuGetVersionV2)));

        /// <summary>
        /// test
        /// </summary>
        public static ITargetDefinition Test(ITargetDefinition _, IXamarinBuild build) => _
            .DependsOn(build.Build)
            .OnlyWhenStatic(() => DirectoryExists(build.TestDirectory))
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
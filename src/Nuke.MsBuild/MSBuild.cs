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
using static Rocket.Surgery.Nuke.VerbosityDictionaries;

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Base build plan for .NET Framework based applications
    /// </summary>
    public abstract class MSBuild : RocketBoosterBuild
    {
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
                            // .SetBinaryLogger(LogsDirectory / "restore.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                            // .SetFileLogger(LogsDirectory / "restore.log", Verbosity)
                            .SetVerbosity(NuGetVerbosityDictionary[Verbosity])
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
                            .SetBinaryLogger(LogsDirectory / "build.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                            .SetFileLogger(LogsDirectory / "build.log", Verbosity)
                            .SetVerbosity(MSBuildVerbosityDictionary[Verbosity])
                            .SetGitVersionEnvironment(GitVersion)
                            .SetAssemblyVersion(GitVersion.AssemblySemVer));
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
                                .SetBinaryLogger(LogsDirectory / "test.binlog", IsLocalBuild ? MSBuildBinaryLogImports.None : MSBuildBinaryLogImports.Embed)
                                .SetFileLogger(LogsDirectory / "test.log", Verbosity)
                                .EnableNoRestore()
                                .SetLogger($"trx")
                                .SetVerbosity(DotNetVerbosityDictionary[Verbosity])
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
                foreach (var project in Solution.WherePackable())
                {
                    NuGetTasks
                        .NuGetPack(settings =>
                            settings
                                .SetTargetPath(project.Path)
                                .SetConfiguration(Configuration)
                                .SetGitVersionEnvironment(GitVersion)
                                .SetOutputDirectory(NuGetPackageDirectory)
                                .SetVerbosity(NuGetVerbosityDictionary[Verbosity])
                                .SetSymbols(true)
                                .SetBuild(true));
                }
            });
    }
}

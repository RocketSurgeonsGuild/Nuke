using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Base build plan for .NET Core based applications
    /// </summary>
    public abstract class DotNetCoreBuild<T> : RocketBoosterBuild<T>
        where T : Configuration
    {
        protected DotNetCoreBuild(Func<T> configurationDefault) : base(configurationDefault) { }

        /// <summary>
        /// dotnet restore
        /// </summary>
        public static ITargetDefinition Restore(ITargetDefinition _, IDotNetCoreBuild<T> build) => _
           .Description("Restores the dependencies.")
           .DependsOn(build.Clean)
           .DependsOn(build.DotnetToolRestore)
           .Executes(
                () =>
                {
                    DotNetRestore(
                        s => s
                           .SetProjectFile(build.Solution)
                           .SetDisableParallel(true)
                           .SetDefaultLoggers(build.LogsDirectory / "restore.log")
                           .SetGitVersionEnvironment(build.GitVersion)
                    );
                }
            );

        /// <summary>
        /// dotnet build
        /// </summary>
        public static ITargetDefinition Build(ITargetDefinition _, IDotNetCoreBuild<T> build) => _
           .Description("Builds all the projects.")
           .DependsOn(build.Restore)
           .Executes(
                () =>
                {
                    DotNetBuild(
                        s => s
                           .SetProjectFile(build.Solution)
                           .SetDefaultLoggers(build.LogsDirectory / "build.log")
                           .SetGitVersionEnvironment(build.GitVersion)
                           .SetConfiguration(build.Configuration)
                           .EnableNoRestore()
                    );
                }
            );

        /// <summary>
        /// dotnet test
        /// </summary>
        public static ITargetDefinition Test(ITargetDefinition _, IDotNetCoreBuild<T> build) => Test(true)(_, build);

        /// <summary>
        /// dotnet test
        /// </summary>
        public static Func<ITargetDefinition, IDotNetCoreBuild<T>, ITargetDefinition> Test(bool useDataCollector)
            => (_, build) => _
               .Description("Executes all the unit tests.")
               .After(build.Build)
               .DependentFor(build.Pack)
               .DependentFor(build.Generate_Code_Coverage_Reports)
               .Triggers(build.Generate_Code_Coverage_Reports)
               .OnlyWhenStatic(() => DirectoryExists(build.TestDirectory))
               .OnlyWhenDynamic(() => build.TestDirectory.GlobFiles("**/*.csproj").Count > 0)
               .WhenSkipped(DependencyBehavior.Execute)
               .Executes(
                    () =>
                    {
                        EnsureCleanDirectory(build.TestResultsDirectory);
                        build.CoverageDirectory.GlobFiles("*.cobertura.xml", "*.opencover.xml", "*.json", "*.info")
                           .Where(x => Guid.TryParse(Path.GetFileName(x).Split('.')[0], out var _))
                           .ForEach(DeleteFile);
                    }
                )
               .Executes(
                    async () =>
                    {
                        var runsettings = build.TestDirectory / "coverlet.runsettings";
                        if (!FileExists(runsettings))
                        {
                            runsettings = TemporaryDirectory / "default.runsettings";
                            if (!FileExists(runsettings))
                            {
                                using var tempFile = File.Open(runsettings, FileMode.CreateNew);
                                await typeof(DotNetCoreBuild<>).Assembly
                                       .GetManifestResourceStream("Rocket.Surgery.Nuke.DotNetCore.default.runsettings")!
                                   .CopyToAsync(tempFile)
                                   .ConfigureAwait(false);
                            }
                        }

                        DotNetTest(
                            s => s
                               .SetProjectFile(build.Solution)
                               .SetDefaultLoggers(build.LogsDirectory / "test.log")
                               .SetGitVersionEnvironment(build.GitVersion)
                               .SetConfiguration("Debug")
                               .EnableNoRestore()
                               .SetLogger("trx")
                                // DeterministicSourcePaths being true breaks coverlet!
                               .SetProperty("DeterministicSourcePaths", "false")
                               .SetResultsDirectory(build.TestResultsDirectory)
                               .When(
                                    !useDataCollector,
                                    x => x
                                       .SetProperty("CollectCoverage", "true")
                                       .SetProperty("CoverageDirectory", build.CoverageDirectory)
                                )
                               .When(
                                    useDataCollector,
                                    x => x
                                       .SetProperty("CollectCoverage", "false")
                                       .SetDataCollector("XPlat Code Coverage")
                                       .SetSettingsFile(runsettings)
                                )
                        );

                        // Ensure anything that has been dropped in the test results from a collector is
                        // into the coverage directory
                        foreach (var file in build.TestResultsDirectory
                           .GlobFiles("**/*.cobertura.xml")
                           .Where(x => Guid.TryParse(Path.GetFileName(x.Parent), out var _))
                           .SelectMany(coverage => coverage.Parent.GlobFiles("*.*")))
                        {
                            var folderName = Path.GetFileName(file.Parent);
                            var extensionPart = string.Join(".", Path.GetFileName(file).Split('.').Skip(1));
                            CopyFile(
                                file,
                                build.CoverageDirectory / $"{folderName}.{extensionPart}",
                                FileExistsPolicy.OverwriteIfNewer
                            );
                        }
                    }
                );

        /// <summary>
        /// dotnet pack
        /// </summary>
        public static ITargetDefinition Pack(ITargetDefinition _, IDotNetCoreBuild<T> build) => _
           .Description("Packs all the NuGet packages.")
           .DependsOn(build.Build)
           .Executes(
                () =>
                {
                    DotNetPack(
                        s => s
                           .SetProject(build.Solution)
                           .SetDefaultLoggers(build.LogsDirectory / "pack.log")
                           .SetGitVersionEnvironment(build.GitVersion)
                           .SetConfiguration(build.Configuration)
                           .EnableNoRestore()
                           .EnableNoBuild()
                           .SetOutputDirectory(build.NuGetPackageDirectory)
                    );
                }
            );

        /// <summary>
        /// This will ensure that all local dotnet tools are installed
        /// </summary>
        public Target DotnetToolRestore => _ => _
           .After(Clean)
           .OnlyWhenStatic(() => FileExists(RootDirectory / ".config/dotnet-tools.json"))
           .Unlisted()
           .Executes(() => DotNet("tool restore"));
    }

    public class DotNetCoreBuild : DotNetCoreBuild<Configuration>
    {
        public DotNetCoreBuild()
            : base(() => IsLocalBuild ? Configuration.Debug : Configuration.Release) { }
    }
}
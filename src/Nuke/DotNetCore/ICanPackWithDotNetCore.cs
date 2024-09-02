using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines a `dotnet pack` run with logging and configuration output to the nuget package directory
/// </summary>
public interface ICanPackWithDotNetCore : IHaveBuildTarget,
    IHaveNuGetPackages,
    IHaveTestTarget,
    IHavePackTarget,
    IHaveSolution,
    IHaveOutputLogs,
    IHaveGitVersion,
    IHaveCleanTarget,
    IHaveConfiguration,
    ICan
{
    /// <summary>
    ///     dotnet pack
    /// </summary>
    [NonEntryTarget]
    public Target DotnetCorePack => d => d
                                        .Description("Packs all the NuGet packages.")
                                        .Unlisted()
                                        .After(Clean)
                                        .TryDependentFor<IHavePackTarget>(a => a.Pack)
                                        .TryDependsOn<IHaveBuildTarget>(b => b.Build)
                                        .TryAfter<IHaveRestoreTarget>(a => a.Restore)
                                        .Executes(
                                             () => DotNetTasks.DotNetPack(
                                                 s => s
                                                     .SetProject(Solution)
                                                     .SetDefaultLoggers(LogsDirectory / "pack.log")
                                                     .SetGitVersionEnvironment(GitVersion)
                                                     .SetConfiguration(Configuration)
                                                     .EnableNoRestore()
                                                     .EnableNoBuild()
                                                     .AddProperty("PackageOutputPath", NuGetPackageDirectory)
                                             )
                                         );
}
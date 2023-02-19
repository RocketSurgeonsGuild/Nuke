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
                                          IHaveConfiguration,
                                          ICan
{
    /// <summary>
    ///     dotnet pack
    /// </summary>
    public Target CorePack => _ => _
                                  .Description("Packs all the NuGet packages.")
                                  .DependsOn(Build)
                                  .Executes(
                                       () => DotNetTasks.DotNetPack(
                                           s => s.SetProject(Solution)
                                                 .SetDefaultLoggers(LogsDirectory / "pack.log")
                                                 .SetGitVersionEnvironment(GitVersion)
                                                 .SetConfiguration(Configuration)
                                                 .EnableNoRestore()
                                                 .EnableNoBuild()
                                                 .AddProperty("PackageOutputPath", NuGetPackageDirectory)
                                       )
                                   );
}

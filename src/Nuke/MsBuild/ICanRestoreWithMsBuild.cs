using Nuke.Common.Tools.NuGet;

namespace Rocket.Surgery.Nuke.MsBuild;

/// <summary>
///     Defines a restore task using msbuild
/// </summary>
public interface ICanRestoreWithMsBuild : IHaveRestoreTarget, IHaveCleanTarget, IHaveSolution, ICan
{
    /// <summary>
    ///     nuget restore
    /// </summary>
    Target NetRestore => d => d
                                    .DependsOn(Clean)
                                    .Unlisted()
                                    .Executes(
                                         () => NuGetTasks
                                            .NuGetRestore(
                                                 settings =>
                                                     settings
                                                        .SetSolutionDirectory(Solution)
                                                        .EnableNoCache()
                                             )
                                     );
}

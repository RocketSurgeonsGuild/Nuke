using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Rocket.Surgery.Nuke.DotNetCore;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Defines targets for a library project that tracks apis using the Microsoft.CodeAnalysis.PublicApiAnalyzers package
/// </summary>
public interface IHavePublicApis : ICanDotNetFormat
{
    /// <summary>
    /// Determine if Unshipped apis should always be pushed the Shipped file used in lint-staged to automatically update the shipped file
    /// </summary>
    public bool ShouldMoveUnshippedToShipped => IsLocalBuild;

    /// <summary>
    /// All the projects that depend on the Microsoft.CodeAnalysis.PublicApiAnalyzers package
    /// </summary>
    public IEnumerable<Project> PublicApiAnalyzerProjects => Solution
                                                            .AllProjects
                                                            .Where(z => z.HasPackageReference("Microsoft.CodeAnalysis.PublicApiAnalyzers"));

    private IEnumerable<AbsolutePath> LintPublicApiShippedFiles => PublicApiAnalyzerProjects
                                                                  .SelectMany(
                                                                       project => new[]
                                                                           { GetShippedFilePath(project.Directory), GetUnshippedFilePath(project.Directory) }
                                                                   )
#pragma warning disable CA1860
                                                                  .Where(file => !LintPaths.Any() || LintPaths.Any(z => z == file));
#pragma warning restore CA1860


    private static AbsolutePath GetShippedFilePath(AbsolutePath directory) => directory / "PublicAPI.Shipped.txt";
    private static AbsolutePath GetUnshippedFilePath(AbsolutePath directory) => directory / "PublicAPI.Unshipped.txt";

    /// <summary>
    /// Setup to lint the public api projects
    /// </summary>
    [UsedImplicitly]
    public Target LintPublicApiAnalyzers => d =>
        d
           .DependentFor(Lint)
           .Executes(
                async () =>
                {
                    foreach (var project in PublicApiAnalyzerProjects)
                    {
                        var shippedFilePath = GetShippedFilePath(project.Directory);
                        var unshippedFilePath = GetUnshippedFilePath(project.Directory);
                        if (!shippedFilePath.FileExists())
                        {
                            await File.WriteAllTextAsync(shippedFilePath, "#nullable enable");
                        }

                        if (!unshippedFilePath.FileExists())
                        {
                            await File.WriteAllTextAsync(unshippedFilePath, "#nullable enable");
                        }

#pragma warning disable CA1860
                        if (LintPaths.Any())
#pragma warning restore CA1860
                        {
                            DotNetTasks.DotNet($"format {project.Path} --diagnostics=RS0016 --include {string.Join(",", LintPaths)}");
                        }
                        else
                        {
                            DotNetTasks.DotNet($"format {project.Path} --diagnostics=RS0016");
                        }
                    }
                }
            );

    /// <summary>
    /// Ensure the shipped file is up to date
    /// </summary>
    [UsedImplicitly]
    public Target MoveUnshippedToShipped => d =>
        d
           .After(LintPublicApiAnalyzers)
           .OnlyWhenDynamic(() => ShouldMoveUnshippedToShipped)
           .DependentFor(Lint)
           .Executes(
                async () =>
                {
                    foreach (var project in PublicApiAnalyzerProjects)
                    {
                        Logger.Info($"Moving unshipped to shipped for {project.Name}");
                        var shippedFilePath = GetShippedFilePath(project.Directory);
                        var unshippedFilePath = GetUnshippedFilePath(project.Directory);

                        var shipped = await GetLines(shippedFilePath);
                        var unshipped = await GetLines(unshippedFilePath);
                        foreach (var item in unshipped)
                        {
                            if (item is not { Length: > 0 }) continue;
                            shipped.Add(item);
                        }

                        shipped.Sort();
                        shipped.Insert(0, "#nullable enable");
                        await File.WriteAllLinesAsync(shippedFilePath, shipped);
                        await File.WriteAllTextAsync(unshippedFilePath, "#nullable enable");
                    }

                    async static Task<List<string>> GetLines(AbsolutePath path) => path.FileExists()
                        ? ( await File.ReadAllLinesAsync(path) )
                         .Where(z => z != "#nullable enable")
                         .ToList()
                        : new List<string>();
                }
            );
}

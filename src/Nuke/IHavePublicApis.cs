using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

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
                                                                 .SelectMany(project => new[] { GetShippedFilePath(project.Directory), GetUnshippedFilePath(project.Directory) })
        .Where(file => LintFiles is { Count: 0 } || LintFiles.Any(z => z == file));


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

                        if (LintFiles.Count > 0)
                        {
                            DotNetTasks.DotNet($"format {project.Path} --diagnostics=RS0016 --include {string.Join(",", LintFiles)}");
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
           .OnlyWhenDynamic(() => LintPublicApiShippedFiles.Any())
           .DependentFor(Lint)
           .Executes(
                async () =>
                {
                    foreach (var project in PublicApiAnalyzerProjects)
                    {
                        var shippedFilePath = GetShippedFilePath(project.Directory);
                        var unshippedFilePath = GetUnshippedFilePath(project.Directory);

                        var shipped = ( shippedFilePath.FileExists() ? await File.ReadAllLinesAsync(shippedFilePath) : Array.Empty<string>() )
                                     .Where(z => z != "#nullable enable")
                                     .ToList();
                        var unshipped = ( unshippedFilePath.FileExists() ? await File.ReadAllLinesAsync(shippedFilePath) : Array.Empty<string>() )
                                       .Where(z => z != "#nullable enable")
                                       .ToList();
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
                }
            );
}

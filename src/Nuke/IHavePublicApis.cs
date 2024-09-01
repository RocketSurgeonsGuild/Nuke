using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke.ProjectModel;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines targets for a library project that tracks apis using the Microsoft.CodeAnalysis.PublicApiAnalyzers package
/// </summary>
public interface IHavePublicApis : IHaveSolution, ICanLint, IHaveOutputLogs
{
    private static AbsolutePath GetShippedFilePath(AbsolutePath directory)
    {
        return directory / "PublicAPI.Shipped.txt";
    }

    private static AbsolutePath GetUnshippedFilePath(AbsolutePath directory)
    {
        return directory / "PublicAPI.Unshipped.txt";
    }

    /// <summary>
    ///     Determine if Unshipped apis should always be pushed the Shipped file used in lint-staged to automatically update the shipped file
    /// </summary>
    public bool ShouldMoveUnshippedToShipped => true;

    /// <summary>
    ///     Setup to lint the public api projects
    /// </summary>
    public Target ShipPublicApis => d =>
                                        d.Triggers(LintPublicApiAnalyzers);

    /// <summary>
    ///     Setup to lint the public api projects
    /// </summary>
    [UsedImplicitly]
    public Target LintPublicApiAnalyzers => d =>
                                                d
                                                   .TriggeredBy(Lint)
                                                   .Before(PostLint)
                                                   .Unlisted()
                                                   .Executes(
                                                        async () =>
                                                        {
                                                            await foreach (var project in GetPublicApiAnalyzerProjects())
                                                            {
                                                                var shippedFilePath = GetShippedFilePath(project.Directory);
                                                                var unshippedFilePath = GetUnshippedFilePath(project.Directory);
                                                                if (!shippedFilePath.FileExists())
                                                                    await File.WriteAllTextAsync(shippedFilePath, "#nullable enable");

                                                                if (!unshippedFilePath.FileExists())
                                                                    await File.WriteAllTextAsync(unshippedFilePath, "#nullable enable");

                                                                var arguments = new Arguments()
                                                                               .Add("format")
                                                                               .Add("analyzers")
                                                                               .Add(
                                                                                    "--verbosity {value}",
                                                                                    Verbosity
                                                                                       .MapVerbosity(MSBuildVerbosity.Normal)
                                                                                       .ToString()
                                                                                       .ToLowerInvariant()
                                                                                )
                                                                               .Add("--no-restore")
                                                                               .Add(
                                                                                    "--binarylog {value}",
                                                                                    LogsDirectory / $"public-api-format.{project.Name}.binlog"
                                                                                )
                                                                               .Add("--diagnostics {value}", "RS0016");

                                                                DotNetTasks.DotNet(arguments.RenderForExecution(), RootDirectory, logInvocation: false);
                                                            }
                                                        }
                                                    );

    /// <summary>
    ///     Ensure the shipped file is up to date
    /// </summary>
    [UsedImplicitly]
    public Target MoveUnshippedToShipped => d =>
                                                d
                                                   .DependsOn(LintPublicApiAnalyzers)
                                                   .TriggeredBy(LintPublicApiAnalyzers)
                                                   .OnlyWhenDynamic(() => ShouldMoveUnshippedToShipped)
                                                   .Executes(
                                                        async () =>
                                                        {
                                                            await foreach (var project in GetPublicApiAnalyzerProjects())
                                                            {
                                                                Log.Logger.Information("Moving unshipped to shipped for {ProjectName}", project.Name);
                                                                var shippedFilePath = GetShippedFilePath(project.Directory);
                                                                var unshippedFilePath = GetUnshippedFilePath(project.Directory);

                                                                var shipped = await GetLines(shippedFilePath);
                                                                var unshipped = await GetLines(unshippedFilePath);
                                                                foreach (var item in unshipped)
                                                                {
                                                                    if (item is not { Length: > 0, }) continue;
                                                                    shipped.Add(item);
                                                                }

                                                                shipped.Sort();
                                                                shipped.Insert(0, "#nullable enable");
                                                                await File.WriteAllLinesAsync(shippedFilePath, shipped);
                                                                await File.WriteAllTextAsync(unshippedFilePath, "#nullable enable");
                                                            }

                                                            static async Task<List<string>> GetLines(AbsolutePath path)
                                                            {
                                                                return path.FileExists()
                                                                    ? ( await File.ReadAllLinesAsync(path) )
                                                                     .Where(z => z != "#nullable enable")
                                                                     .ToList()
                                                                    : new();
                                                            }
                                                        }
                                                    );

    /// <summary>
    ///     All the projects that depend on the Microsoft.CodeAnalysis.PublicApiAnalyzers package
    /// </summary>
    private async IAsyncEnumerable<Project> GetPublicApiAnalyzerProjects()
    {
        if (!LintPaths.HasPaths) yield break;
        foreach (var project in Solution.AllProjects)
        {
            var analysis = await project.AnalyzeProject();
            if (analysis
               .PackageReferences
               .Values
               .SelectMany(z => z.Keys)
               .Any(z => z.Equals("Microsoft.CodeAnalysis.PublicApiAnalyzers", StringComparison.OrdinalIgnoreCase)))
                yield return project;
        }
    }
}

using System.Collections.Concurrent;
using System.Diagnostics;
using Buildalyzer;
using Buildalyzer.Environment;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

namespace Rocket.Surgery.Nuke.ProjectModel;

using Project = Project;
using Solution = Solution;

/// <summary>
///     Extensions for <see cref="Buildalyzer" />.
/// </summary>
public static class BuildalyzerExtensions
{
    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    public static async Task<ProjectAnalyzerModel> Analyze(this Project project, string? targetFramework = null, EnvironmentOptions? options = null)
    {
        var solutionManager = await GetAnalyzerManager(project.Solution, options);
        return await solutionManager.GetProject(project, targetFramework);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    public static Task<SolutionAnalyzerModel> Analyze(this Solution solution, EnvironmentOptions? options = null)
    {
        return GetAnalyzerManager(solution, options);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    public static async IAsyncEnumerable<ProjectAnalyzerModel> AnalyzeAllProjects(
        this Solution solution,
        string? targetFramework = null,
        EnvironmentOptions? options = null
    )
    {
        var manager = await GetAnalyzerManager(solution, options);
        foreach (var item in await manager.Analyze())
        {
            yield return item;
        }
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    public static Task<ProjectAnalyzerModel> AnalyzeBinLog(
        this Project project,
        AbsolutePath binlogPath,
        string? targetFramework = null,
        EnvironmentOptions? options = null
    )
    {
        var solutionManager = new BinLogAnalyzerModel(binlogPath);
        return solutionManager.GetProject(project, targetFramework);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> using the provided binlog and returns the result for the given target framework.
    /// </summary>
    public static BinLogAnalyzerModel AnalyzeBinLog(this AbsolutePath binlogPath)
    {
        return new(binlogPath);
    }

    private static readonly ConcurrentDictionary<Solution, SolutionAnalyzerModel> _solutionReferences = new();
    private static readonly ConcurrentDictionary<string, IAnalyzerResults> _projectResults = new();

    private static Task<SolutionAnalyzerModel> GetAnalyzerManager(Solution solution, EnvironmentOptions? environmentOptions = null)
    {
        return _solutionReferences.TryGetValue(solution, out var manager)
            ? Task.FromResult(manager)
            : Task.Run(
                () =>
                {
                    var sw = Stopwatch.StartNew();
                    Log.Information("Analyzing solution {Solution}", solution.Path);
                    var analyzerManager = new AnalyzerManager(solution.Path);
                    _solutionReferences.TryAdd(solution, new(analyzerManager));
                    sw.Stop();
                    Log.Information("Analyzed solution {Solution} in {Elapsed}", solution.Path, sw.Elapsed);

                    return new SolutionAnalyzerModel(analyzerManager, environmentOptions);
                }
            );
    }
}
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Buildalyzer;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using Serilog.Extensions.Logging;

#pragma warning disable RS0026
namespace Rocket.Surgery.Nuke.ProjectModel;

/// <summary>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </summary>
/// <remarks>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class BinLogAnalyzerModel(string binlogPath) : ICommonAnalyzerModel
{
    private static readonly object _analyzedLock = new();
    private readonly AnalyzerManager _manager = new() { LoggerFactory = new SerilogLoggerFactory(Log.Logger), };
    private readonly Dictionary<string, ProjectAnalyzerResults> _projectAnalyzerResults = [];

    private bool _analyzed;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => binlogPath;

    private Task<ProjectAnalyzerModel> GetProjectImpl(string projectPath, string? targetFramework = null)
    {
        return ICommonAnalyzerModel.GetFromCacheOrAnalyze(
            () => Analyze(projectPath, targetFramework),
            _projectAnalyzerResults,
            projectPath,
            targetFramework
        );
    }

    private async Task<ProjectAnalyzerModel> Analyze(string projectPath, string? targetFramework = null)
    {
        await AnalyzeBinLog();
        return !_projectAnalyzerResults.TryGetValue(projectPath, out var results)
            ? throw new InvalidOperationException($"Project {projectPath} was not found in the binlog")
            : results.GetProjectForTargetFramework(targetFramework);
    }

    private Task AnalyzeBinLog()
    {
        return _analyzed
            ? Task.CompletedTask
            : Task.Run(
                () =>
                {
                    if (_analyzed)
                    {
                        return;
                    }

                    lock (_analyzedLock)
                    {
                        if (_analyzed)
                        {
                            return;
                        }

                        Log.Information("Reading {Log}", binlogPath);
                        var sw = Stopwatch.StartNew();
                        var results = _manager.Analyze(binlogPath);
                        foreach (var result in results.GroupBy(z => z.ProjectFilePath))
                        {
                            var projectAnalyzer = _manager.GetProject(result.Key);
                            _projectAnalyzerResults.Add(result.Key, new(projectAnalyzer, result.ToFrozenSet()));
                        }

                        sw.Stop();
                        _analyzed = true;
                        Log.Information("Read {Log} in {Elapsed}", binlogPath, sw.Elapsed);
                    }
                }
            );
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(Project project)
    {
        return _projectAnalyzerResults.TryGetValue(project.Path, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework())
            : GetProjectImpl(project.Path);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="projectPath"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(AbsolutePath projectPath)
    {
        return _projectAnalyzerResults.TryGetValue(projectPath, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework())
            : GetProjectImpl(projectPath);
    }

    /// <summary>
    ///     Get all the projects from the analyzer manager
    /// </summary>
    /// <returns></returns>
    public IAsyncEnumerable<ProjectAnalyzerModel> GetProjects(bool includeHidden = false)
    {
        return _manager
              .Projects
              .Values
              .Where(z => includeHidden || !z.ProjectFile.Name.StartsWith("."))
              .ToAsyncEnumerable()
              .SelectAwait(async z => await GetProjectImpl(z.ProjectFile.Path));
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(Project project, string targetFramework)
    {
        return _projectAnalyzerResults.TryGetValue(project.Path, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework(targetFramework))
            : GetProjectImpl(project.Path, targetFramework);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(AbsolutePath projectPath, string targetFramework)
    {
        return _projectAnalyzerResults.TryGetValue(projectPath, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework(targetFramework))
            : GetProjectImpl(projectPath, targetFramework);
    }

    /// <summary>
    ///     Get all the projects from the analyzer manager
    /// </summary>
    /// <param name="targetFramework"></param>
    /// <param name="includeHidden"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ProjectAnalyzerModel> GetProjects(string targetFramework, bool includeHidden = false)
    {
        return _manager
              .Projects.Values
              .Where(z => includeHidden || !z.ProjectFile.Name.StartsWith("."))
              .ToAsyncEnumerable()
              .SelectAwait(async z => await GetProjectImpl(z.ProjectFile.Path, targetFramework));
    }

    /// <summary>
    ///     Analyze the solution / binlog
    /// </summary>
    /// <returns></returns>
    public async Task<ImmutableArray<ProjectAnalyzerModel>> Analyze()
    {
        return [.. await GetProjects().ToArrayAsync(),];
    }
}
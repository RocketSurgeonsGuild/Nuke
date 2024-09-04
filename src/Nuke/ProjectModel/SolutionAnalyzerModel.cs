using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Buildalyzer;
using Buildalyzer.Environment;
using Microsoft.Build.Construction;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Rocket.Surgery.Nuke.ProjectModel;

/// <summary>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </summary>
/// <remarks>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class SolutionAnalyzerModel : ICommonAnalyzerModel
{
    private readonly EnvironmentOptions _environmentOptions;
    private readonly Dictionary<string, ProjectAnalyzerResults> _projectAnalyzerResults = [];
    private readonly AnalyzerManager _manager;

    /// <summary>
    ///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
    /// </summary>
    /// <remarks>
    ///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
    /// </remarks>
    public SolutionAnalyzerModel(string solution, EnvironmentOptions? environmentOptions = null)
    {
        _manager = new(solution) { LoggerFactory = new SerilogLoggerFactory(Log.Logger), };
        _environmentOptions = environmentOptions ?? new();
        _environmentOptions.TargetsToBuild.Remove("Clean");
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => _manager.SolutionFilePath;

    /// <summary>
    /// The solution file
    /// </summary>
    public SolutionFile SolutionFile => _manager.SolutionFile;

    private IAsyncEnumerable<ProjectAnalyzerModel> GetProjectsImpl(LogEventLevel logEventLevel, string? targetFramework = null)
    {
        return _manager
              .Projects.Values.ToAsyncEnumerable()
              .SelectAwait(async z => await GetProjectImpl(z.ProjectFile.Path, logEventLevel, targetFramework));
    }

    private Task<ProjectAnalyzerModel> GetProjectImpl(string projectPath, LogEventLevel logEventLevel, string? targetFramework = null)
    {
        return ICommonAnalyzerModel.GetFromCacheOrAnalyze(
            () => Task.Run(
                () =>
                {
                    var projectAnalyzer = _manager.GetProject(projectPath);

                    Log.Write(logEventLevel, "Building project {Project}", projectAnalyzer.ProjectFile.Name);
                    var sw = Stopwatch.StartNew();
                    var environment = projectAnalyzer.EnvironmentFactory.GetBuildEnvironment(targetFramework, _environmentOptions);
                    var results = environment is null ? projectAnalyzer.Build() : projectAnalyzer.Build(environment);
                    var projectResults = new ProjectAnalyzerResults(
                        projectAnalyzer,
                        results.Where(z => z.ProjectFilePath == projectAnalyzer.ProjectFile.Path).ToFrozenSet()
                    );
                    _projectAnalyzerResults.Add(projectPath, projectResults);
                    sw.Stop();
                    Log.Write(logEventLevel, "Built project {Project} in {Elapsed}", projectAnalyzer.ProjectFile.Name, sw.Elapsed);

                    return projectResults.GetProjectForTargetFramework(targetFramework);
                }
            ),
            _projectAnalyzerResults,
            projectPath,
            targetFramework
        );
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(Project project, string? targetFramework = null)
    {
        return _projectAnalyzerResults.TryGetValue(project.Path, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework(targetFramework))
            : GetProjectImpl(project.Path, LogEventLevel.Information, targetFramework);
    }

    /// <summary>
    ///     Loads the project through <see cref="AnalyzerManager" /> and returns the result for the given target framework.
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public Task<ProjectAnalyzerModel> GetProject(AbsolutePath projectPath, string? targetFramework = null)
    {
        return _projectAnalyzerResults.TryGetValue(projectPath, out var results)
            ? Task.FromResult(results.GetProjectForTargetFramework(targetFramework))
            : GetProjectImpl(projectPath, LogEventLevel.Information, targetFramework);
    }

    /// <summary>
    ///     Get all the projects from the analyzer manager
    /// </summary>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ProjectAnalyzerModel> GetProjects(string? targetFramework = null)
    {
        return GetProjectsImpl(LogEventLevel.Information, targetFramework);
    }

    /// <summary>
    ///     Analyze the solution / binlog
    /// </summary>
    /// <returns></returns>
    public async Task<ImmutableArray<ProjectAnalyzerModel>> Analyze()
    {
        Log.Information("Analyzing solution {Solution}", _manager.SolutionFilePath);
        var sw = Stopwatch.StartNew();
        var projects = await GetProjectsImpl(LogEventLevel.Verbose).ToArrayAsync();
        sw.Stop();
        Log.Information("Analyzed solution {Solution} in {Elapsed}", _manager.SolutionFilePath, sw.Elapsed);
        return [.. projects,];
    }
}

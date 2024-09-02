using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using Buildalyzer;
using Buildalyzer.Construction;
using Buildalyzer.Environment;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Nuke.ProjectModel;

/// <summary>
///     A common interface for the analyzer model
/// </summary>
public interface ICommonAnalyzerModel
{
    internal static Task<ProjectAnalyzerModel> GetFromCacheOrAnalyze(
        Func<Task<ProjectAnalyzerModel>> analyze,
        IReadOnlyDictionary<string, ProjectAnalyzerResults> results,
        string projectPath,
        string? targetFramework = null
    )
    {
        return results.TryGetValue(projectPath, out var p)
            ? Task.FromResult(p.GetProjectForTargetFramework(targetFramework))
            : analyze();
    }

    /// <summary>
    ///     Get a project from the analyzer manager
    /// </summary>
    /// <param name="project"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    Task<ProjectAnalyzerModel> GetProject(Project project, string? targetFramework = null);

    /// <summary>
    ///     Get a project from the analyzer manager
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    Task<ProjectAnalyzerModel> GetProject(AbsolutePath projectPath, string? targetFramework = null);

    /// <summary>
    ///     Get all the projects from the analyzer manager
    /// </summary>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    IAsyncEnumerable<ProjectAnalyzerModel> GetProjects(string? targetFramework = null);

    /// <summary>
    ///     Analyze the solution / binlog
    /// </summary>
    /// <returns></returns>
    Task<ImmutableArray<ProjectAnalyzerModel>> Analyze();
}

/// <summary>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </summary>
public class SolutionAnalyzerModel : ICommonAnalyzerModel
{
    private readonly AnalyzerManager _manager;
    private readonly EnvironmentOptions _environmentOptions;
    private readonly Dictionary<string, ProjectAnalyzerResults> _projectAnalyzerResults = new();

    /// <summary>
    ///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
    /// </summary>
    public SolutionAnalyzerModel(AnalyzerManager manager, EnvironmentOptions? environmentOptions = null)
    {
        _manager = manager;
        _environmentOptions = environmentOptions ?? new();
    }

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
        var sw = new Stopwatch();
        var projects = await GetProjectsImpl(LogEventLevel.Verbose).ToArrayAsync();
        sw.Stop();
        Log.Information("Analyzed solution {Solution} in {Elapsed}", _manager.SolutionFilePath, sw.Elapsed);
        return [..projects,];
    }
}

/// <summary>
///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
/// </summary>
public class BinLogAnalyzerModel : ICommonAnalyzerModel
{
    private static readonly object _analyzedLock = new();
    private readonly string _binlogPath;
    private readonly AnalyzerManager _manager = new();
    private readonly Dictionary<string, ProjectAnalyzerResults> _projectAnalyzerResults = new();

    private bool _analyzed;

    /// <summary>
    ///     A wrapper around the Analyzer Manager to provide a more strongly typed model for returning projects and solutions
    /// </summary>
    public BinLogAnalyzerModel(string binlogPath)
    {
        _binlogPath = binlogPath;
    }

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
        if (!_projectAnalyzerResults.TryGetValue(projectPath, out var results))
        {
            throw new InvalidOperationException($"Project {projectPath} was not found in the binlog");
        }

        return results.GetProjectForTargetFramework(targetFramework);
    }

    private Task AnalyzeBinLog()
    {
        return _analyzed
            ? Task.CompletedTask
            : Task.Run(
                () =>
                {
                    if (_analyzed) return;
                    lock (_analyzedLock)
                    {
                        if (_analyzed) return;
                        Log.Information("Reading {Log}", _binlogPath);
                        var sw = Stopwatch.StartNew();
                        var results = _manager.Analyze(_binlogPath);
                        foreach (var result in results.GroupBy(z => z.ProjectFilePath))
                        {
                            var projectAnalyzer = _manager.GetProject(result.Key);
                            _projectAnalyzerResults.Add(result.Key, new(projectAnalyzer, result.ToFrozenSet()));
                        }

                        sw.Stop();
                        _analyzed = true;
                        Log.Information("Read {Log} in {Elapsed}", _binlogPath, sw.Elapsed);
                    }
                }
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
            : GetProjectImpl(project.Path, targetFramework);
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
            : GetProjectImpl(projectPath, targetFramework);
    }

    /// <summary>
    ///     Get all the projects from the analyzer manager
    /// </summary>
    /// <param name="targetFramework"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ProjectAnalyzerModel> GetProjects(string? targetFramework = null)
    {
        return _manager.Projects.Values.ToAsyncEnumerable().SelectAwait(async z => await GetProjectImpl(z.ProjectFile.Path, targetFramework));
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

internal record ProjectAnalyzerResults(IProjectAnalyzer Project, FrozenSet<IAnalyzerResult> Results)
{
    public ProjectAnalyzerModel GetProjectForTargetFramework(string? targetFramework = null)
    {
        targetFramework ??= Results.Reverse().Select(z => z.TargetFramework).First();
        return new(Results.FirstOrDefault(z => z.TargetFramework == targetFramework) ?? returnDefault(Results.Last(), Project.ProjectFile.Path));

        static ProjectAnalyzerModel returnDefault(IAnalyzerResult result, string contextPath)
        {
            Log.Warning(
                "No target framework specified or found for {ContextPath}, returning the last result.  Available target frameworks: {@TargetFrameworks}",
                contextPath,
                result.Analyzer.ProjectFile.TargetFrameworks
            );
            return new(result);
        }
    }
}

/// <summary>
///     A wrapper around the analyzer result to provide a more strongly typed model
/// </summary>
public class ProjectAnalyzerModel : IAnalyzerResult
{
    /// <summary>
    ///     Implicitly convert the model to the project file path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static implicit operator string(ProjectAnalyzerModel model)
    {
        return model.ProjectFilePath;
    }

    private readonly IAnalyzerResult _result;

    /// <summary>
    ///     A wrapper around the analyzer result to provide a more strongly typed model
    /// </summary>
    public ProjectAnalyzerModel(IAnalyzerResult result)
    {
        Project = result.Analyzer;
        _result = result;
    }

    /// <summary>
    ///     The project name
    /// </summary>
    public string Name => ProjectFile.Name;

    /// <summary>
    ///     The project file model
    /// </summary>
    public IProjectFile ProjectFile => Project.ProjectFile;

    /// <summary>
    ///     The project file model
    /// </summary>
    public ProjectAnalyzer Project { get; }

    /// <summary>
    ///     The package id
    /// </summary>

    public string PackageId => GetProperty<string>(nameof(PackageId)) ?? ProjectFile.Name;

    /// <summary>
    ///     Is this project packable
    /// </summary>

    public bool IsPackable => GetProperty<bool>(nameof(IsPackable));

    /// <summary>
    ///     Is this a test project
    /// </summary>
    public bool IsTestProject => GetProperty<bool>(nameof(IsTestProject));

    /// <summary>
    ///     The project file path
    /// </summary>
    public AbsolutePath ProjectFilePath => _result.ProjectFilePath;

    /// <summary>
    ///     The directory of the project file
    /// </summary>
    public AbsolutePath Directory => ProjectFilePath.Parent!;

    /// <summary>
    ///     Get a property from the analyzer result supports a limited number of types
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public T? GetProperty<T>(string name) where T : notnull
    {
        var value = _result.GetProperty(name);
        if (typeof(T) == typeof(bool)) return (T?)(object?)( value is "enable" or "true" );
        if (typeof(T) == typeof(string)) return (T?)(object?)value;
        throw new NotSupportedException(typeof(T).FullName);
    }

    string IAnalyzerResult.GetProperty(string name)
    {
        return _result.GetProperty(name);
    }

    /// <summary>
    ///     The source project analyzer
    /// </summary>
    public ProjectAnalyzer Analyzer => _result.Analyzer;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IProjectItem[]> Items => _result.Items;

    AnalyzerManager IAnalyzerResult.Manager => _result.Manager;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> PackageReferences => _result.PackageReferences;

    /// <inheritdoc />
    string IAnalyzerResult.ProjectFilePath => _result.ProjectFilePath;

    /// <inheritdoc />
    public Guid ProjectGuid => _result.ProjectGuid;

    /// <inheritdoc />
    public IEnumerable<string> ProjectReferences => _result.ProjectReferences;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Properties => _result.Properties;

    /// <inheritdoc />
    public string[] References => _result.References;

    /// <inheritdoc />
    public string[] AnalyzerReferences => _result.AnalyzerReferences;

    /// <inheritdoc />
    public string[] SourceFiles => _result.SourceFiles;

    /// <inheritdoc />
    public bool Succeeded => _result.Succeeded;

    /// <inheritdoc />
    public string TargetFramework => _result.TargetFramework;

    /// <inheritdoc />
    public string[] PreprocessorSymbols => _result.PreprocessorSymbols;

    /// <inheritdoc />
    public string[] AdditionalFiles => _result.AdditionalFiles;

    /// <inheritdoc />
    public string Command => _result.Command;

    /// <inheritdoc />
    public string CompilerFilePath => _result.CompilerFilePath;

    /// <inheritdoc />
    public string[] CompilerArguments => _result.CompilerArguments;
}
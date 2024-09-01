using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Buildalyzer;
using Buildalyzer.Environment;
using Nuke.Common.IO;
using Serilog;
using Serilog.Context;

namespace Rocket.Surgery.Nuke.ProjectModel;

using Project = global::Nuke.Common.ProjectModel.Project;
using Solution = global::Nuke.Common.ProjectModel.Solution;

/// <summary>
/// Extensions for <see cref="Buildalyzer"/>.
/// </summary>
public static class BuildalyzerExtensions
{
    private static readonly ConcurrentDictionary<Solution, IAnalyzerManager> _solutionReferences = new();
    private static readonly ConcurrentDictionary<string, IAnalyzerResults> _projectResults = new();

    /// <summary>
    /// Loads the project through <see cref="AnalyzerManager"/> and returns the result for the given target framework.
    /// </summary>
    public static async Task<ProjectAnalyzerModel> AnalyzeProject(this Project project, string? targetFramework = null, EnvironmentOptions? options = null)
    {
        {
            var analyzerManager = await GetAnalyzerManager(project.Solution);
            var analyzer = analyzerManager.GetProject(project.Path);
            targetFramework ??= analyzer.ProjectFile.TargetFrameworks.Last();
            if (_projectResults.TryGetValue(project.Path, out var results))
            {
                if (results.TryGetTargetFramework(targetFramework, out var result))
                    return new(result);
                throw new InvalidOperationException($"Failed to find target framework {targetFramework}");
            }
        }

        return await Task.Run(
            async () =>
            {
                var analyzerManager = await GetAnalyzerManager(project.Solution);
                var analyzer = analyzerManager.GetProject(project.Path);
                var environment = analyzer.EnvironmentFactory.GetBuildEnvironment(targetFramework, options ?? new());

                var sw = Stopwatch.StartNew();
                Log.Information("Building project {Project}", project.Name);
                var r = environment is null ? analyzer.Build() : analyzer.Build(environment);
                _projectResults.TryAdd(project.Path, r);
                sw.Stop();
                Log.Information("Built project {Project} in {Elapsed}", project.Name, sw.Elapsed);

                return targetFramework is { Length: > 0 }
                    ? r.TryGetTargetFramework(targetFramework, out var result)
                        ? new ProjectAnalyzerModel(result)
                        : throw new InvalidOperationException($"Failed to find target framework {targetFramework}")
                    : new(r.Results.Last());
            }
        );
    }

    /// <summary>
    /// Loads the project through <see cref="AnalyzerManager"/> using the provided binlog and returns the result for the given target framework.
    /// </summary>
    public static Task<ProjectAnalyzerModel> AnalyzeBinLog(this AbsolutePath binLogPath, string? targetFramework = null)
    {
        return _projectResults.TryGetValue(binLogPath, out var results)
            ? results.TryGetTargetFramework(targetFramework ?? results.TargetFrameworks.Last(), out var result)
                ? Task.FromResult(new ProjectAnalyzerModel(result))
                : throw new InvalidOperationException($"Failed to find target framework {targetFramework}")
            : Task.Run(
                () =>
                {
                    var analyzerManager = new AnalyzerManager();
                    var r = analyzerManager.Analyze(binLogPath);
                    _projectResults.TryAdd(binLogPath, r);

                    return targetFramework is { Length: > 0 }
                        ? r.TryGetTargetFramework(targetFramework, out var rr)
                            ? new ProjectAnalyzerModel(rr)
                            : throw new InvalidOperationException($"Failed to find target framework {targetFramework}")
                        : new(r.Results.Last());
                }
            );
    }

    private static Task<IAnalyzerManager> GetAnalyzerManager(Solution solution)
    {
        return _solutionReferences.TryGetValue(solution, out var manager)
            ? Task.FromResult(manager)
            : Task.Factory.StartNew(
                static (state) =>
                {
                    if (state is not Solution solution) throw new InvalidOperationException("Invalid state");
                    using (LogContext.PushProperty("Solution", solution.Path))
                    {
                        var sw = Stopwatch.StartNew();
                        Log.Information("Loading solution {Solution}", solution.Path);
                        var manager = new AnalyzerManager(solution.Path);
                        _solutionReferences.TryAdd(solution, manager);
                        sw.Stop();
                        Log.Information("Loaded solution {Solution} in {Elapsed}", solution.Path, sw.Elapsed);
                        return (IAnalyzerManager)manager;
                    }
                },
                solution
            );
    }
}

using System.Runtime.CompilerServices;
using Buildalyzer;
using Buildalyzer.Environment;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke.ProjectModel;

using Project = global::Nuke.Common.ProjectModel.Project;
using Solution = global::Nuke.Common.ProjectModel.Solution;

/// <summary>
/// Extensions for <see cref="Buildalyzer"/>.
/// </summary>
public static class BuildalyzerExtensions
{
    private static readonly ConditionalWeakTable<Solution, AnalyzerManager> _solutionReferences = new();
    private static readonly ConditionalWeakTable<string, IAnalyzerResults> _projectResults = new();

    /// <summary>
    /// Loads the project through <see cref="AnalyzerManager"/> and returns the result for the given target framework.
    /// </summary>
    public static async Task<IAnalyzerResult> AnalyzeProject(this Project project, string? targetFramework = null, EnvironmentOptions? options = null)
    {
        {
            var analyzerManager = await GetAnalyzerManager(project.Solution);
            var analyzer = analyzerManager.GetProject(project.Path);
            targetFramework ??= analyzer.ProjectFile.TargetFrameworks.Last();
            if (_projectResults.TryGetValue(project.Path, out var results))
            {
                if (results.TryGetTargetFramework(targetFramework, out var result))
                    return result;
                throw new InvalidOperationException($"Failed to find target framework {targetFramework}");
            }
        }

        return await Task.Run(
            async () =>
            {
                var analyzerManager = await GetAnalyzerManager(project.Solution);
                var analyzer = analyzerManager.GetProject(project.Path);
                var environment = analyzer.EnvironmentFactory.GetBuildEnvironment(targetFramework, options ?? new());

                var r = environment is null ? analyzer.Build() : analyzer.Build(environment);
                _projectResults.Add(project.Path, r);

                return r.TryGetTargetFramework(targetFramework, out var result)
                    ? result
                    : throw new InvalidOperationException($"Failed to find target framework {targetFramework}");
            }
        );
    }

    /// <summary>
    /// Loads the project through <see cref="AnalyzerManager"/> using the provided binlog and returns the result for the given target framework.
    /// </summary>
    public static Task<IAnalyzerResult> AnalyzeBinLog(this AbsolutePath binLogPath, string? targetFramework = null)
    {
        return _projectResults.TryGetValue(binLogPath, out var results)
            ? results.TryGetTargetFramework(targetFramework ?? results.TargetFrameworks.Last(), out var result)
                ? Task.FromResult(result)
                : throw new InvalidOperationException($"Failed to find target framework {targetFramework}")
            : Task.Run(
                () =>
                {
                    var analyzerManager = new AnalyzerManager();
                    var r = analyzerManager.Analyze(binLogPath);
                    _projectResults.Add(binLogPath, r);

                    return r.TryGetTargetFramework(targetFramework ?? r.TargetFrameworks.Last(), out var re)
                            ? re
                            : throw new InvalidOperationException($"Failed to find target framework {targetFramework}");
                }
            );
    }

    private static Task<AnalyzerManager> GetAnalyzerManager(Solution solution)
    {
        return _solutionReferences.TryGetValue(solution, out var manager)
            ? Task.FromResult(manager)
            : Task.Factory.StartNew(
                static (state) =>
                {
                    if (state is not Solution solution) throw new InvalidOperationException("Invalid state");
                    var manager = new AnalyzerManager(solution.Path);
                    _solutionReferences.Add(solution, manager);
                    return manager;
                },
                solution
            );
    }
}

using System.Collections.Immutable;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

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
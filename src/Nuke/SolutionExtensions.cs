using Nuke.Common.ProjectModel;
using Rocket.Surgery.Nuke.ProjectModel;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Extensions for finding projects in solutions
/// </summary>
public static class SolutionExtensions
{
    /// <summary>
    ///     Gets the <see cref="Project" /> that are marked packable.
    /// </summary>
    /// <param name="solution">The solution.</param>
    /// <returns>An enumerable of projects.</returns>
    public static async IAsyncEnumerable<ProjectAnalyzerModel> WherePackable(this Solution solution)
    {
        foreach (var project in solution.AllProjects)
        {
            var analyzeProject = await project.Analyze();
            if (analyzeProject is { IsPackable: true, IsTestProject: false, }) yield return analyzeProject;
        }
    }

    /// <summary>
    ///     Gets the test projects.
    /// </summary>
    /// <param name="solution">The solution.</param>
    /// <returns></returns>
    public static async IAsyncEnumerable<ProjectAnalyzerModel> GetTestProjects(this Solution solution)
    {
        foreach (var project in solution.AllProjects)
        {
            var analyzeProject = await project.Analyze();
            if (analyzeProject.IsTestProject) yield return analyzeProject;
        }
    }
}
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
    public static IAsyncEnumerable<ProjectAnalyzerModel> WherePackable(this Solution solution)
    {
        return solution.AnalyzeAllProjects().Where(project => project is { IsPackable: true, IsTestProject: false, });
    }

    /// <summary>
    ///     Gets the test projects.
    /// </summary>
    /// <param name="solution">The solution.</param>
    /// <returns></returns>
    public static IAsyncEnumerable<ProjectAnalyzerModel> GetTestProjects(this Solution solution)
    {
        return solution
              .AnalyzeAllProjects()
              .Where(z => z.IsTestProject);
    }
}
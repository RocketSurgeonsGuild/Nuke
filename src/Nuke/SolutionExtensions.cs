using System;
using System.Collections.Generic;
using System.Linq;
using Buildalyzer;
using Nuke.Common.ProjectModel;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Extensions for finding projects in solutions
    /// </summary>
    public static class SolutionExtensions
    {
        /// <summary>
        /// Gets the <see cref="Project" /> that are marked packable.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns>An enumerable of projects.</returns>
        public static IEnumerable<Project> WherePackable(this Solution solution) => solution.AllProjects.Where(z => z.GetProperty<bool>("IsPackable"));

        /// <summary>
        /// Gets the test projects.
        /// </summary>
        /// <param name="solution">The solution.</param>
        /// <returns></returns>
        public static IEnumerable<Project> GetTestProjects(this Solution solution) => solution.AllProjects.Where(z => z.GetProperty<bool>("IsTestProject"));
    }
}
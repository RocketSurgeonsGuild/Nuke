using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines a solution
    /// </summary>
    public interface IHaveSolution
    {
        /// <summary>
        /// The solution currently being build
        /// </summary>
        [Solution]
        public Solution Solution => InjectionUtility.GetInjectionValue(() => Solution);
    }
}
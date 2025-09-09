using Nuke.Common.ProjectModel;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a solution
/// </summary>
public interface IHaveSolution : IHave
{
    /// <summary>
    ///     The solution currently being build
    /// </summary>
    [Solution]
    Solution Solution => TryGetValue(() => Solution)!;
}

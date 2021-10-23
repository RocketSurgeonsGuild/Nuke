using Nuke.Common.ProjectModel;
using Nuke.Common.ValueInjection;

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
    public Solution Solution => ValueInjectionUtility.TryGetValue(() => Solution)!;
}

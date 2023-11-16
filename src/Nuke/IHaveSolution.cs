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
    // ReSharper disable once NullableWarningSuppressionIsUsed
    public Solution Solution => TryGetValue(() => Solution)!;
}

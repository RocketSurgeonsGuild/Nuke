namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
[Obsolete("Interface no longer in use, consider using jetbrains global tool!")]
public interface ICanLint : IHaveSolution
{
    /// <summary>
    ///     The old lint target
    /// </summary>
    public Target Lint => _ => _;
}

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
public interface ICanLint : INukeBuild
{
    /// <summary>
    ///     The old lint target
    /// </summary>
    public Target Lint => t => t;

    /// <summary>
    /// The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ")]
    public IReadOnlyList<string> LintFiles => TryGetValue(() => LintFiles) ?? Array.Empty<string>();
}

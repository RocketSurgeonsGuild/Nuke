using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
public interface ICanLint : INukeBuild
{
    /// <summary>
    ///     The lint target
    /// </summary>
    public Target Lint => t => t;

    /// <summary>
    ///     A lint target that runs last
    /// </summary>
    public Target PostLint => t => t.Unlisted().After(Lint).TriggeredBy(Lint);

    /// <summary>
    ///     The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ", Name = "lint-files")]
#pragma warning disable CA1819
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? Array.Empty<string>();
#pragma warning restore CA1819

    /// <summary>
    ///     The lint paths rooted as an absolute path.
    /// </summary>
    public IEnumerable<AbsolutePath> LintPaths => PrivateLintFiles.Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : RootDirectory / z);
}

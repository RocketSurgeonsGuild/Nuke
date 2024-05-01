using Nuke.Common.IO;
using Nuke.Common.Tools.Git;

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
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    public Target LintGitAdd =>
        t => t
            .Unlisted()
            .After(PostLint)
            .TriggeredBy(PostLint)
            .OnlyWhenDynamic(() => LintPaths.Any())
            .Executes(() => GitTasks.Git($"add {string.Join(" ", LintPaths.Select(z => $"\"{RootDirectory.GetRelativePathTo(z)}\""))}"));

    /// <summary>
    ///     The lint paths rooted as an absolute path.
    /// </summary>
    public IEnumerable<AbsolutePath> LintPaths => PrivateLintFiles.Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : RootDirectory / z);

    /// <summary>
    ///     The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ", Name = "lint-files")]
    #pragma warning disable CA1819
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? [];
    #pragma warning restore CA1819
}
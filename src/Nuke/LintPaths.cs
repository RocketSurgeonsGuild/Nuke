using System.Collections.Frozen;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     The trigger that the lint paths were created for
/// </summary>
public enum LintTrigger
{
    /// <summary>
    ///     The paths were created for a specified files
    /// </summary>
    Specific,

    /// <summary>
    ///     The local staged files
    /// </summary>
    Staged,

    /// <summary>
    ///     A pull request
    /// </summary>
    PullRequest,

    /// <summary>
    ///     No files.
    /// </summary>
    /// <remarks>
    ///     This could means tasks could be run on all files or no files, whatever is needed.
    /// </remarks>
    None,
}

/// <summary>
///     Lint paths container
/// </summary>
public class LintPaths
{
    //    public FrozenSet<AbsolutePath> Paths => _paths;

    /// <summary>
    ///     convert to the set
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static implicit operator FrozenSet<AbsolutePath>(LintPaths paths)
    {
        return paths.HasPaths ? paths._paths : Enumerable.Empty<AbsolutePath>().ToFrozenSet();
    }

    private readonly FrozenSet<AbsolutePath> _paths;

    /// <summary>
    ///     Lint paths container
    /// </summary>
    /// <param name="matcher">The matcher to include / exclude files on a global level even ones that might be checked in</param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    public LintPaths(Matcher matcher, LintTrigger trigger, string message, IEnumerable<string> paths)
    {
        Trigger = trigger;
        Message = message;

        _paths = paths
                .Select(z => z.Trim())
                .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
                .Match(matcher)
                .ToFrozenSet();
    }

    /// <summary>
    ///     The trigger that the lint paths were created for
    /// </summary>
    public LintTrigger Trigger { get; }

    /// <summary>
    ///     Message about how the paths were resolved
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public bool HasPaths => Trigger != LintTrigger.None;

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public IEnumerable<AbsolutePath> Paths => _paths;

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public IEnumerable<RelativePath> RelativePaths => _paths.GetRelativePaths();

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public IEnumerable<RelativePath> Glob(Matcher matcher)
    {
        return _paths.Match(matcher).GetRelativePaths();
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public IEnumerable<RelativePath> Glob(string matcher)
    {
        return _paths.Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(matcher)).GetRelativePaths();
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public IEnumerable<AbsolutePath> GlobAbsolute(Matcher matcher)
    {
        return _paths.Match(matcher);
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public IEnumerable<AbsolutePath> GlobAbsolute(string matcher)
    {
        return _paths.Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(matcher));
    }
}
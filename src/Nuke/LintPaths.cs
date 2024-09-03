using System.Collections.Frozen;
using System.Collections.Immutable;
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
    private readonly Dictionary<Matcher, ImmutableArray<AbsolutePath>> _matches = new();

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

        Paths =
        [
            ..paths
             .Select(z => z.Trim())
             .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
             .Match(matcher)
        ];
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
    public bool Active => Trigger != LintTrigger.None;

    /// <summary>
    /// Determine if the task should run in the current context
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public bool IsLocalLintOrMatches(Matcher matcher) => Trigger == LintTrigger.None && NukeBuild.IsLocalBuild || matcher.Match(Paths.Select(z => z.ToString())).HasMatches;

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public ImmutableArray<AbsolutePath> Paths { get; }

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public ImmutableArray<RelativePath> RelativePaths => [..Paths.GetRelativePaths()];

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableArray<RelativePath> Glob(Matcher matcher) => [..GlobAbsolute(matcher).GetRelativePaths()];

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableArray<RelativePath> Glob(string matcher)
    {
        return [..Paths.Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(matcher)).GetRelativePaths()];
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableArray<AbsolutePath> GlobAbsolute(Matcher matcher) =>
        _matches.TryGetValue(matcher, out var results)
            ? results
            : _matches[matcher] = [..Paths.Match(matcher)];

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableArray<AbsolutePath> GlobAbsolute(string matcher)
    {
        return [..Paths.Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(matcher))];
    }
}

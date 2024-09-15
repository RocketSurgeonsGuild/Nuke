using System.Collections.Immutable;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;

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
    private readonly Matcher _matcher;
    private readonly Dictionary<(Matcher Matcher, bool AllPaths), ImmutableArray<AbsolutePath>> _matches = new();
    private readonly Lazy<ImmutableArray<AbsolutePath>> _allPaths;
    private readonly Lazy<ImmutableArray<AbsolutePath>> _paths;

    /// <summary>
    ///     Lint paths container
    /// </summary>
    /// <param name="matcher">The matcher to include / exclude files on a global level even ones that might be checked in</param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    public LintPaths(Matcher matcher, LintTrigger trigger, string message, IEnumerable<string> paths)
    {
        _matcher = matcher;
        Trigger = trigger;
        Message = message;

        _paths = new (() =>
        [
            ..paths
             .Select(z => z.Trim())
             .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z),
        ]);

        _allPaths = new(
            () =>
            [
                ..matcher
                 .Match(
                      GitTasks
                         .Git("ls-files", logOutput: false, logInvocation: false)
                         .Select(z => z.Text.Trim())
                         .Select(z => Path.IsPathRooted(z) ? z : (string)( NukeBuild.RootDirectory / z ))
                  )
                 .Files
                 .Select(z => AbsolutePath.Create(z.Path))
                 .OrderBy(z => z),
            ]
        );
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
    ///     Are there any paths?
    /// </summary>
    public IEnumerable<AbsolutePath> Paths => _paths.Value.Match(_matcher);

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public IEnumerable<RelativePath> RelativePaths => _paths.Value.Match(_matcher).GetRelativePaths();

    /// <summary>
    ///     Determine if the task should run in the current context
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public bool IsLocalLintOrMatches(Matcher matcher)
    {
        return ( Trigger == LintTrigger.None && NukeBuild.IsLocalBuild ) || matcher.Match(Paths.Select(z => z.ToString())).HasMatches;
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="allPaths"></param>
    /// <returns></returns>
    public ImmutableArray<RelativePath> Glob(Matcher matcher, bool allPaths = false)
    {
        return [..GlobAbsolute(matcher, allPaths).GetRelativePaths(),];
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="allPaths"></param>
    /// <returns></returns>
    public ImmutableArray<RelativePath> Glob(string pattern, bool allPaths = false)
    {
        return [..( allPaths ? _allPaths.Value : Paths ).Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern)).GetRelativePaths(),];
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="allPaths"></param>
    /// <returns></returns>
    public ImmutableArray<AbsolutePath> GlobAbsolute(Matcher matcher, bool allPaths = false)
    {
        return _matches.TryGetValue(( matcher, allPaths ), out var results)
            ? results
            : _matches[( matcher, allPaths )] = [..Paths.Match(matcher),];
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="allPaths"></param>
    /// <returns></returns>
    public ImmutableArray<AbsolutePath> GlobAbsolute(string pattern, bool allPaths = false)
    {
        return [..( allPaths ? _allPaths.Value : Paths ).Match(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern)),];
    }
}

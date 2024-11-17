using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Lint paths container
/// </summary>
public class LintPaths
{
    private readonly Matcher _matcher;
    private readonly ConditionalWeakTable<Matcher, ImmutableList<AbsolutePath>> _pathsCache = new();
    private readonly ConditionalWeakTable<Matcher, ImmutableList<RelativePath>> _relativeCache = new();
    private readonly Lazy<LintPaths> _allPaths;
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

        _paths = new(
            () =>
            [
                ..paths
                 .Select(z => z.Trim())
                 .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z),
            ]
        );

        _allPaths = new(
            () =>
            {
                return new(
                    _matcher,
                    LintTrigger.None,
                    Message,
                    GitTasks
                       .Git("ls-files", NukeBuild.RootDirectory, logOutput: false, logInvocation: false)
                       .Select(z => z.Text.Trim())
                       .Select(z => Path.IsPathRooted(z) ? z : (string)( NukeBuild.RootDirectory / z ))
                );
            }
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
    ///     The filtered paths
    /// </summary>
    public IEnumerable<AbsolutePath> Paths => _paths.Value.Match(_matcher);

    /// <summary>
    ///     All the paths
    /// </summary>
    public LintPaths AllPaths => _allPaths.Value;

    /// <summary>
    ///     The relative paths
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
    /// <returns></returns>
    public ImmutableList<RelativePath> Glob(Matcher matcher)
    {
        return _relativeCache.GetValue(matcher, m => [.. _paths.Value.Match(m).GetRelativePaths(),]);
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public ImmutableList<RelativePath> Glob(string pattern) => Glob(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern));

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableList<AbsolutePath> GlobAbsolute(Matcher matcher)
    {
        return _pathsCache.GetValue(matcher, m => [.. _paths.Value.Match(m),]);
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public ImmutableList<AbsolutePath> GlobAbsolute(string pattern) => GlobAbsolute(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern));
}

using System.Collections.Immutable;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Lint paths container
/// </summary>
public sealed class LintPaths
{
    /// <summary>
    ///     Create a new lint paths container
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static LintPaths Create(Matcher matcher, LintTrigger trigger, string message, IEnumerable<string> paths) => new(
        trigger,
        message,
        new(
            () => ( trigger is LintTrigger.None
                      ? GitTasks
                       .Git("ls-files", NukeBuild.RootDirectory, logOutput: false, logInvocation: false)
                       .Select(z => z.Text.Trim())
                      : paths )
                 .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
                 .Match(matcher)
                 .ToImmutableList()
        )
    );

    /// <summary>
    ///     Lint paths container
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static LintPaths Create(Matcher matcher, LintTrigger trigger, string message, IEnumerable<AbsolutePath> paths) =>
        new(trigger, message, new(() => paths.Match(matcher).ToImmutableList()));

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public ImmutableList<RelativePath> Glob(Matcher matcher) => [.. Paths.Match(matcher).GetRelativePaths()];

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
    public ImmutableList<AbsolutePath> GlobAbsolute(Matcher matcher) => [.. Paths.Match(matcher)];

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public ImmutableList<AbsolutePath> GlobAbsolute(string pattern) => GlobAbsolute(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern));

    /// <summary>
    ///     Determine if the task should run in the current context
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public bool IsLocalLintOrMatches(Matcher matcher) => ( Trigger == LintTrigger.None && NukeBuild.IsLocalBuild ) || matcher.Match(Paths.Select(z => z.ToString())).HasMatches;

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public bool Active => Trigger != LintTrigger.None;

    /// <summary>
    ///     Message about how the paths were resolved
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     The filtered paths
    /// </summary>
    public IEnumerable<AbsolutePath> Paths => _paths.Value;

    /// <summary>
    ///     The relative paths
    /// </summary>
    public IEnumerable<RelativePath> RelativePaths => _paths.Value.GetRelativePaths();

    /// <summary>
    ///     The trigger that the lint paths were created for
    /// </summary>
    public LintTrigger Trigger { get; }

    /// <summary>
    ///     Lint paths container
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    private LintPaths(LintTrigger trigger, string message, Lazy<ImmutableList<AbsolutePath>> paths)
    {
        Trigger = trigger;
        Message = message;
        _paths = paths;
    }

    private readonly Lazy<ImmutableList<AbsolutePath>> _paths;
}

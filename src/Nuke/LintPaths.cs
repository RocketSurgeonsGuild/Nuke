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
    private readonly Lazy<LintPaths> _allPaths;
    private readonly Lazy<ImmutableList<AbsolutePath>> _paths;

    /// <summary>
    ///   Create a new lint paths container
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static LintPaths Create(Matcher matcher, LintTrigger trigger, string message, IEnumerable<string> paths) => new(
        trigger,
        message,
        new(() => paths.Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z).Match(matcher).ToImmutableList()),
        CreateAllPaths(matcher, message)
    );

    /// <summary>
    ///   Lint paths container
    /// </summary>
    /// <param name="matcher"></param>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static LintPaths Create(Matcher matcher, LintTrigger trigger, string message, IEnumerable<AbsolutePath> paths) =>
        new(trigger, message, new(paths.ToImmutableList), CreateAllPaths(matcher, message));

    private static Lazy<LintPaths> CreateAllPaths(Matcher matcher, string message) => new(
        () =>
        {
            return new(
                LintTrigger.None,
                message,
                new(
                    () => GitTasks
                         .Git("ls-files", NukeBuild.RootDirectory, logOutput: false, logInvocation: false)
                         .Select(z => z.Text.Trim())
                         .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
                         .ToImmutableList()
                ),
                null
            );
        }
    );

    /// <summary>
    ///     Lint paths container
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    /// <param name="allPaths"></param>
    private LintPaths(LintTrigger trigger, string message, Lazy<ImmutableList<AbsolutePath>> paths, Lazy<LintPaths>? allPaths)
    {
        Trigger = trigger;
        Message = message;
        _paths = paths;
        _allPaths = allPaths ?? new Lazy<LintPaths>(() => new(LintTrigger.None, message, new(() => ImmutableList<AbsolutePath>.Empty), null));
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
    public IEnumerable<AbsolutePath> Paths => _paths.Value;

    /// <summary>
    ///     All the paths
    /// </summary>
    public LintPaths AllPaths => _allPaths.Value;

    /// <summary>
    ///     The relative paths
    /// </summary>
    public IEnumerable<RelativePath> RelativePaths => _paths.Value.GetRelativePaths();

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
        return [.._paths.Value.Match(matcher).GetRelativePaths()];
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
        return [.._paths.Value.Match(matcher)];
    }

    /// <summary>
    ///     Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public ImmutableList<AbsolutePath> GlobAbsolute(string pattern) => GlobAbsolute(new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(pattern));
}

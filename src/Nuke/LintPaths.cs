using System.Collections.Frozen;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Serilog;

namespace Rocket.Surgery.Nuke;

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
    /// <param name="pathsProvided"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    public LintPaths(Matcher matcher, bool pathsProvided, string message, IEnumerable<string> paths)
    {
        Message = message;
        HasPaths = pathsProvided;

        _paths = paths
                .Select(z => z.Trim())
                .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
                .Match(matcher)
                .ToFrozenSet();
    }

    /// <summary>
    ///     Message about how the paths were resolved
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Are there any paths?
    /// </summary>
    public bool HasPaths { get; }

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

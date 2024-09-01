using System.Collections.Frozen;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Lint paths container
/// </summary>
public class LintPaths
{
    private readonly bool _pathsProvided;
    private readonly FrozenSet<AbsolutePath> _paths;

    /// <summary>
    /// Lint paths container
    /// </summary>
    /// <param name="matcher">The matcher to include / exclude files on a global level even ones that might be checked in</param>
    /// <param name="pathsProvided"></param>
    /// <param name="message"></param>
    /// <param name="paths"></param>
    public LintPaths(Matcher matcher, bool pathsProvided, string message, IEnumerable<string> paths)
    {
        Message = message;
        _pathsProvided = pathsProvided;

        var intermediatePaths = paths
                               .Select(z => z.Trim())
                               .Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : NukeBuild.RootDirectory / z)
                               .Select(z => z.ToString());

        _paths = matcher.Match(intermediatePaths).Files.Select(z => AbsolutePath.Create(z.Path)).ToFrozenSet();
    }

    /// <summary>
    /// Message about how the paths were resolved
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Are there any paths?
    /// </summary>
    public bool HasPaths => _pathsProvided;

    /// <summary>
    /// Are there any paths?
    /// </summary>
    public IEnumerable<AbsolutePath> Paths => _paths;

    /// <summary>
    /// Glob against a given matcher to included / exclude files
    /// </summary>
    /// <param name="matcher"></param>
    /// <returns></returns>
    public IEnumerable<AbsolutePath> Glob(Matcher matcher)
    {
        var result = matcher.Match(_paths.Select(z => z.ToString()));
        return result.HasMatches ? result.Files.Select(z => AbsolutePath.Create(z.Path)) : [];
    }

//    public FrozenSet<AbsolutePath> Paths => _paths;

    /// <summary>
    ///  convert to the set
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static implicit operator FrozenSet<AbsolutePath>(LintPaths paths) =>
        paths._pathsProvided ? paths._paths : Enumerable.Empty<AbsolutePath>().ToFrozenSet();
}

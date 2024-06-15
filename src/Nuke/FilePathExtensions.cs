using System.Collections.Concurrent;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Extensions related to file paths
/// </summary>
public static class FilePathExtensions
{
    /// <summary>
    ///     Returns the first directory that exists on disk
    /// </summary>
    /// <remarks>
    ///     Caches the result for faster lookups later
    /// </remarks>
    /// <param name="path"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static AbsolutePath PickDirectory(AbsolutePath path, params AbsolutePath[] paths)
    {
        return PickDirectory([path, .. paths,]);
    }

    /// <summary>
    ///     Returns the first directory that exists on disk
    /// </summary>
    /// <remarks>
    ///     Caches the result for faster lookups later
    /// </remarks>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static AbsolutePath PickDirectory(IEnumerable<AbsolutePath> paths)
    {
        var absolutePaths = paths as AbsolutePath[] ?? paths.ToArray();
        foreach (var path in absolutePaths)
        {
            if (Cache.ContainsKey(path)) return path;

            if (!path.DirectoryExists()) continue;

            Cache.TryAdd(path, default);

            return path;
        }

        return absolutePaths.First();
    }

    /// <summary>
    ///     Returns the first file that exists on disk
    /// </summary>
    /// <remarks>
    ///     Caches the result for faster lookups later
    /// </remarks>
    /// <param name="path"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static AbsolutePath PickFile(AbsolutePath path, params AbsolutePath[] paths)
    {
        return PickFile([path, .. paths,]);
    }

    /// <summary>
    ///     Returns the first file that exists on disk
    /// </summary>
    /// <remarks>
    ///     Caches the result for faster lookups later
    /// </remarks>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static AbsolutePath PickFile(IEnumerable<AbsolutePath> paths)
    {
        var absolutePaths = paths as AbsolutePath[] ?? paths.ToArray();
        foreach (var path in absolutePaths)
        {
            if (Cache.ContainsKey(path)) return path;

            if (!path.FileExists()) continue;

            Cache.TryAdd(path, default);

            return path;
        }

        return absolutePaths.First();
    }

    private static readonly ConcurrentDictionary<AbsolutePath, object?> Cache = new();
}
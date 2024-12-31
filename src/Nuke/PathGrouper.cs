using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Groups paths into groups that are less than the max command line length
/// </summary>
public static class PathGrouper
{
    /// <summary>
    ///     Groups paths into groups that are less than the max command line length
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static IEnumerable<ImmutableList<RelativePath>> GroupPaths(ImmutableList<RelativePath> paths)
    {
        var currentGroup = ImmutableList.CreateBuilder<RelativePath>();
        var currentLength = 0;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return paths;
            yield break;
        }

        foreach (var path in paths)
        {
            var pathLength = path.ToString().Length;
            currentGroup.Add(path);
            currentLength += pathLength + 1; // +1 for the space or separator

            if (currentLength + pathLength + 1 <= MaxCommandLineLength)
            {
                continue;
            }

            yield return currentGroup.ToImmutable();
            currentGroup.Clear();
            currentLength = 0;
        }

        if (!currentGroup.Any())
        {
            yield break;
        }

        yield return currentGroup.ToImmutable();
        currentGroup.Clear();
    }

    private const int MaxCommandLineLength = 7500;
}

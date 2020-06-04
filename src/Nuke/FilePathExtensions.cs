using System.Collections.Concurrent;
using System.Linq;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Extensions related to file paths
    /// </summary>
    public static class FilePathExtensions
    {
        private static readonly ConcurrentDictionary<AbsolutePath, AbsolutePath> _cache =
            new ConcurrentDictionary<AbsolutePath, AbsolutePath>();

        /// <summary>
        /// Returns the first directory that exists on disk
        /// </summary>
        /// <remarks>
        /// Caches the result for faster lookups later
        /// </remarks>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static AbsolutePath PickDirectory(params AbsolutePath[] paths)
        {
            foreach (var path in paths)
            {
                if (_cache.TryGetValue(path, out _))
                    return path;
                if (!FileSystemTasks.DirectoryExists(path))
                {
                    continue;
                }

                foreach (var p in paths)
                    _cache.TryAdd(p, path);
                return path;
            }

            return paths.First();
        }
    }
}
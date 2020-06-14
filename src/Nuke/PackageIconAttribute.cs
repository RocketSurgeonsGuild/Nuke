using System.IO;
using JetBrains.Annotations;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Injects the path to the downloaded package icon.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    public sealed class PackageIconAttribute : DownloadFileAttribute
    {
        /// <summary>
        /// Ensures that the icon at the given url is downloaded into the specified filePath
        /// </summary>
        /// <param name="url">The Url to download</param>
        /// <param name="filePath">The file path to download to, defaults to TemporaryDirectory / packageicon.[ext]</param>
        public PackageIconAttribute(string url, string? filePath = null)
            : base(url, filePath == null ? "packageicon" + Path.GetExtension(url) : (AbsolutePath)filePath)
            => Type = "Package Icon";
    }
}
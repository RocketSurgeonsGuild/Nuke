using Nuke.Common;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// A common sample directory
    /// </summary>
    public interface IComprehendSamples : IComprehend
    {
        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        public AbsolutePath SampleDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "sample",
            NukeBuild.RootDirectory / "samples"
        );
    }
}
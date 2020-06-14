using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines a directory for nuget packages that should be pushed should go into
    /// </summary>
    public interface IHaveNuGetPackages : IHaveArtifacts
    {
        /// <summary>
        /// The directory where nuget packages will be placed
        /// </summary>
        public AbsolutePath NuGetPackageDirectory => ArtifactsDirectory / "nuget";
    }
}
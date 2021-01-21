using Nuke.Common;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    public interface IHaveDocs : IHaveArtifacts
    {
        public AbsolutePath DocumentationDirectory => NukeBuild.RootDirectory / "docs";

        public AbsolutePath DocumentationsOutputDirectory => ArtifactsDirectory / "docs";
    }
}
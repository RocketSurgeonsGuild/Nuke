using Nuke.Common;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// The directory where templates should be placed.
    /// </summary>
    public interface IComprehendTemplates
    {
        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        public AbsolutePath TemplatesDirectory => FilePathExtensions.PickDirectory(
            NukeBuild.RootDirectory / "template",
            NukeBuild.RootDirectory / "templates"
        );
    }
}
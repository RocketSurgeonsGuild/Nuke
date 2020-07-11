using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ValueInjection;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines the artifacts output directory.
    /// </summary>
    public interface IHaveArtifacts : IHave
    {
        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where artifacts are to be dropped", Name = "Artifacts")]
        public AbsolutePath ArtifactsDirectory => EnvironmentInfo.GetVariable<AbsolutePath>("Artifacts")
         ?? ValueInjectionUtility.TryGetValue(() => ArtifactsDirectory)
         ?? NukeBuild.RootDirectory / "artifacts";
    }
}
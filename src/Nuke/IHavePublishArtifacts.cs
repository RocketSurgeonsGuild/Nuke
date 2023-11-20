using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the publish output directory, this is used to staged published applications and so on.
/// </summary>
public interface IHavePublishArtifacts : IHaveArtifacts
{
    /// <summary>
    ///     The directory where publish output should be placed
    /// </summary>
    public AbsolutePath PublishDirectory => ArtifactsDirectory / "publish";
}

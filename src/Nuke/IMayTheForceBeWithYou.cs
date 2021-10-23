namespace Rocket.Surgery.Nuke;

/// <summary>
///     Includes a force flag that can be used to ensure caches or the disk is cleaned up more than is normally required
/// </summary>
public interface IMayTheForceBeWithYou : INukeBuild
{
    /// <summary>
    ///     Force a clean build, otherwise leave some incremental build pieces
    /// </summary>
    [Parameter("Force a clean build")]
    public bool Force { get; }
}

using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines the restore target
    /// </summary>
    public interface IHaveRestoreTarget
    {
        /// <summary>
        /// The Restore Target
        /// </summary>
        Target Restore { get; }
    }
}
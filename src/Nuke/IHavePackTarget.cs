using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines the pack target
    /// </summary>
    public interface IHavePackTarget
    {
        /// <summary>
        /// The Pack Target
        /// </summary>
        Target Pack { get; }
    }
}
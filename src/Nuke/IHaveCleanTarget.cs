using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines a clean target
    /// </summary>
    public interface IHaveCleanTarget
    {
        /// <summary>
        /// The Clean Target
        /// </summary>
        Target Clean { get; }
    }
}
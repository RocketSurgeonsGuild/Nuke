using Nuke.Common;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines the test target
    /// </summary>
    public interface IHaveTestTarget
    {
        /// <summary>
        /// The Test Target
        /// </summary>
        Target Test { get; }
    }
}
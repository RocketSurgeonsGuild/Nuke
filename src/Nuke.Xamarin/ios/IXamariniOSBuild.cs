using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin iOS based applications
    /// </summary>
    public interface IXamariniOSBuild : IXamarinBuild
    {
        /// <summary>
        /// Target platform for iOS build.
        /// </summary>
         TargetPlatform TargetPlatform { get; }

        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        Target XamariniOS { get; }
    }
}
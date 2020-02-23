using Nuke.Common;
using Nuke.Common.IO;

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
        /// Gets the path for the info plist.
        /// </summary>
        AbsolutePath InfoPlist { get; }

        /// <summary>
        /// Modifies the InfoPlist for the iOS target.
        /// </summary>
        Target ModifyInfoPlist { get; }

        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        Target XamariniOS { get; }
    }
}
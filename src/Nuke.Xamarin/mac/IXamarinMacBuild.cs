using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin {
    /// <summary>
    /// Base build plan for Xamarin macOS based applications
    /// </summary>
    public interface IXamarinMacBuild : IXamarinBuild
    {
        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        Target XamarinMac { get; }
    }
}
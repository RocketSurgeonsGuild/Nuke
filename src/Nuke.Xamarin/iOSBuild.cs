using Nuke.Common;
#pragma warning disable CA1822

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin.iOS based applications
    /// </summary>
    public abstract class iOSBuild : XamarinBuild
    {
        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        public Target XamariniOS => _ => _;
    }
}
using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin based applications
    /// </summary>
    public interface IMacBuild : IRocketBoosterBuild
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        Target Restore { get; }

        /// <summary>
        /// msbuild
        /// </summary>
        Target Build { get; }

        /// <summary>
        /// xunit test
        /// </summary>
        Target Test { get; }
    }
}
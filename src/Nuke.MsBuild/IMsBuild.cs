using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke.MsBuild
{
    /// <summary>
    /// Base build plan for .NET Framework based applications
    /// </summary>
    public interface IMsBuild<T> : IRocketBoosterBuild<T>
        where T : Configuration
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

        /// <summary>
        /// nuget pack
        /// </summary>
        Target Pack { get; }

        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        AbsolutePath NuspecDirectory { get; }
    }

    public interface IMsBuild : IMsBuild<Configuration>
    {
    }
}
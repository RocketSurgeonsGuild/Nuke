using Nuke.Common;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Common interface to allow dotnet build targets to be extended from in later builds
    /// </summary>
    public interface IDotNetCoreBuild : IRocketBoosterBuild
    {
        /// <summary>
        /// This will ensure that all local dotnet tools are installed
        /// </summary>
        Target DotnetToolRestore { get; }

        /// <summary>
        /// dotnet restore
        /// </summary>
        Target Restore { get; }

        /// <summary>
        /// dotnet build
        /// </summary>
        Target Build { get; }

        /// <summary>
        /// dotnet test
        /// </summary>
        Target Test { get; }

        /// <summary>
        /// dotnet build
        /// </summary>
        Target Pack { get; }
    }
}

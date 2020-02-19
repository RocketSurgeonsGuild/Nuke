using Nuke.Common;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Base build plan for .NET Core based applications
    /// </summary>
    public interface IDotNetCoreBuild<T> : IRocketBoosterBuild<T>
        where T : Configuration
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

    public interface IDotNetCoreBuild : IDotNetCoreBuild<Configuration>
    {
    }
}
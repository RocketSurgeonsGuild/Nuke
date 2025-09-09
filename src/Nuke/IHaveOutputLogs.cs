using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a logs directory where structured build and other logs can be placed.
/// </summary>
public interface IHaveOutputLogs : IHaveArtifacts
{
    /// <summary>
    ///     The directory where logs will be placed
    /// </summary>
    AbsolutePath LogsDirectory => ArtifactsDirectory / "logs";
}

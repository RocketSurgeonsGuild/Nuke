using Nuke.Common.Execution;
using Nuke.Common.IO;
using Serilog;
using static Nuke.Common.IO.HttpTasks;

#pragma warning disable CA1019, CA1813
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Injects the path to the downloaded package icon.
/// </summary>
/// <remarks>
///     Ensures that the icon at the given url is downloaded into the specified filePath
/// </remarks>
/// <param name="url">The Url to download</param>
/// <param name="filePath">The file path to download to inside the temporary directory</param>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class DownloadFileAttribute(string url, string filePath) : BuildExtensionAttributeBase, IOnBuildInitialized
{
    /// <inheritdoc />
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (_filePath.FileExists()) return;

        Log.Verbose(
            "Downloading {Type} {Url} to {Path}",
            Type,
            _url,
            NukeBuild.RootDirectory.GetRelativePathTo(_filePath)
        );
        HttpDownloadFile(_url, _filePath);
        Log.Information(
            "Downloaded {Type} {Url} to {Path}",
            Type,
            _url,
            NukeBuild.RootDirectory.GetRelativePathTo(_filePath)
        );
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;

    /// <summary>
    ///     The type of a given file to make logging look more specific
    /// </summary>
    public string Type { get; set; } = "File";

    private readonly AbsolutePath _filePath = filePath is null
        ? throw new ArgumentNullException(nameof(filePath))
        : NukeBuild.TemporaryDirectory / filePath;

    private readonly string _url = url ?? throw new ArgumentNullException(nameof(url));
}

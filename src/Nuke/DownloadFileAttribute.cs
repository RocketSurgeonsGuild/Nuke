using Nuke.Common.Execution;
using Nuke.Common.IO;
using Serilog;
using static Nuke.Common.IO.HttpTasks;

#pragma warning disable CA1019
#pragma warning disable CA1813
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Injects the path to the downloaded package icon.
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class DownloadFileAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    private readonly string _url;
    private readonly AbsolutePath _filePath;

    /// <summary>
    ///     Ensures that the icon at the given url is downloaded into the specified filePath
    /// </summary>
    /// <param name="url">The Url to download</param>
    /// <param name="filePath">The file path to download to inside the temporary directory</param>
    public DownloadFileAttribute(string url, string filePath)
    {
        _url = url ?? throw new ArgumentNullException(nameof(url));
        _filePath = filePath == null
            ? throw new ArgumentNullException(nameof(filePath))
            : NukeBuild.TemporaryDirectory / filePath;
    }

    /// <summary>
    ///     The type of a given file to make logging look more specific
    /// </summary>
    public string Type { get; set; } = "File";

    /// <inheritdoc />
    public void OnBuildInitialized(
        IReadOnlyCollection<ExecutableTarget> executableTargets,
        IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (!_filePath.FileExists())
        {
            Log.Verbose(
                "Downloading {Type} {Url} to {Path}",
                Type,
                _url, NukeBuild.RootDirectory.GetRelativePathTo(_filePath)
            );
            HttpDownloadFile(_url, _filePath);
            Log.Information(
                "Downloaded {Type} {Url} to {Path}",
                Type,
                _url, NukeBuild.RootDirectory.GetRelativePathTo(_filePath)
            );
        }
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;
}

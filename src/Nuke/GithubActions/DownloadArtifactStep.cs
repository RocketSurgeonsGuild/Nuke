namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Download a given artifact
/// </summary>
[PublicAPI]
public class DownloadArtifactStep : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public DownloadArtifactStep(string name) : base(name) => Uses = "actions/download-artifact@v4";

    /// <summary>
    ///     Gets or sets the name of the artifact to download. If unspecified, all artifacts for the run are downloaded.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the destination path. Supports basic tilde expansion. Optional. Default is $GITHUB_WORKSPACE.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     Gets or sets a glob pattern to the artifacts that should be downloaded. Ignored if name is specified.
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether multiple artifacts are merged. If true, the downloaded artifacts will be in the same directory specified by path.
    ///     If false, the downloaded artifacts will be extracted into individual named directories within the specified path. Optional. Default is 'false'.
    /// </summary>
    public bool? MergeMultiple { get; set; }

    /// <summary>
    ///     Gets or sets the GitHub token used to authenticate with the GitHub API. This is required when downloading artifacts from a different repository or from a
    ///     different workflow run.
    /// </summary>
    public string? GithubToken { get; set; }

    /// <summary>
    ///     Gets or sets the repository owner and the repository name joined together by "/". If github-token is specified, this is the repository that artifacts will
    ///     be downloaded from.
    /// </summary>
    public string? Repository { get; set; }

    /// <summary>
    ///     Gets or sets the id of the workflow run where the desired download artifact was uploaded from. If github-token is specified, this is the run that artifacts
    ///     will be downloaded from.
    /// </summary>
    public string? RunId { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
#pragma warning disable CA1308
        WithProperties(x => x.Kebaberize());
#pragma warning restore CA1308
        base.Write(writer);
    }

    /// <inheritdoc />
    protected override string ComputeStepName(string name) => $"🚀 {name}";
}

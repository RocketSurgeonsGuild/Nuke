namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The upload artifact step
/// </summary>
[PublicAPI]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class UploadArtifactStep : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public UploadArtifactStep(string name) : base(name) => Uses = "actions/upload-artifact@v4";

    /// <summary>
    ///     Gets or sets the name of the artifact to upload. Optional. Default is 'artifact'.
    /// </summary>
    public string Name { get; set; } = "artifact";

    /// <summary>
    ///     Gets or sets a file, directory or wildcard pattern that describes what to upload. Required.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    ///     Gets or sets the desired behavior if no files are found using the provided path. Optional. Default is 'warn'.
    /// </summary>
    public string? IfNoFilesFound { get; set; }

    /// <summary>
    ///     Gets or sets the duration after which artifact will expire in days. Optional. Defaults to repository settings.
    /// </summary>
    public int? RetentionDays { get; set; }

    /// <summary>
    ///     Gets or sets the level of compression for Zlib to be applied to the artifact archive. Optional. Default is '6'.
    /// </summary>
    public int? CompressionLevel { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether an artifact with a matching name will be deleted before a new one is uploaded. Optional. Default is 'false'.
    /// </summary>
    public bool? Overwrite { get; set; }

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
#pragma warning disable CA1308
        WithProperties(x => x.Kebaberize());
#pragma warning restore CA1308
        base.Write(writer);
    }

    /// <inheritdoc />
    protected override string ComputeStepName(string name) => $"🏺 {name}";
}

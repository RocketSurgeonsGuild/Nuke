namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Download a given artifact
/// </summary>
public class DownloadArtifactSet : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public DownloadArtifactSet(string name) : base(name)
    {
        Uses = "actions/download-artifact@v1";
    }

    /// <summary>
    ///     The artifact name to download
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The path of the artifact to download
    /// </summary>
    public string? Path { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
#pragma warning disable CA1308
        WithProperties(x => x.Underscore().Camelize().ToLowerInvariant());
#pragma warning restore CA1308
        base.Write(writer);
    }

    /// <inheritdoc />
    protected override string ComputeStepName(string name)
    {
        return $"🚀 {name}";
    }
}

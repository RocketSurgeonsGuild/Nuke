namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The upload artifact step
/// </summary>
public class UploadArtifactStep : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public UploadArtifactStep(string name) : base(name)
    {
        Uses = "actions/upload-artifact@v3";
    }

    /// <summary>
    ///     The name of artifact to upload
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The path of the artifact to upload
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
        return $"🏺 {name}";
    }
}
using Nuke.Common.CI;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The concurrency settings for a workflow or job
/// </summary>
public class RocketSurgeonsGithubActionsConcurrency : ConfigurationEntity
{
    /// <summary>
    ///     The concurrency group
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    ///     Cancel existing jobs
    /// </summary>
    public bool? CancelInProgress { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        using var _ = writer.Indent();
        if (!string.IsNullOrWhiteSpace(Group))
        {
            writer.WriteValue(new("group", Group));
        }

        if (CancelInProgress.HasValue)
        {
            writer.WriteLine($"cancel-in-progress: {CancelInProgress.Value.ToString().ToLowerInvariant()}");
        }
    }
}
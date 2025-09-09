using Nuke.Common.CI;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The job run defaults
/// </summary>
public class RocketSurgeonsGithubActionsDefaultsRun : ConfigurationEntity
{
    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        using var _ = writer.Indent();
        if (!string.IsNullOrWhiteSpace(WorkingDirectory))
            writer.WriteLine($"working-directory: {WorkingDirectory}");
        if (!string.IsNullOrWhiteSpace(Shell?.ToString()))
            writer.WriteLine($"shell: {Shell}");
    }

    /// <summary>
    ///     The shell
    /// </summary>
    public GithubActionShell? Shell { get; set; }

    /// <summary>
    ///     The working directory where the script is run
    /// </summary>
    public string? WorkingDirectory { get; set; }
}

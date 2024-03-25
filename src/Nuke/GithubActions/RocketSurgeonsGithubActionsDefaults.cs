using Nuke.Common.CI;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The job defaults
/// </summary>
public class RocketSurgeonsGithubActionsDefaults : ConfigurationEntity
{
    /// <summary>
    ///     The defaults of run
    /// </summary>
    public RocketSurgeonsGithubActionsDefaultsRun? Run { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        using var _ = writer.Indent();
        if (Run is null) return;
        writer.WriteLine("run:");
        Run.Write(writer);
    }
}
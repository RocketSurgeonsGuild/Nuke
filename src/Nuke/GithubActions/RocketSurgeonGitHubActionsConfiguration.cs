using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;

#pragma warning disable CA1002
#pragma warning disable CA1308
#pragma warning disable CA2227
// ReSharper disable CollectionNeverUpdated.Global
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The Github actions configuration entity
/// </summary>
public class RocketSurgeonGitHubActionsConfiguration : ConfigurationEntity
{
    // ReSharper disable once NullableWarningSuppressionIsUsed
    /// <summary>
    ///     The name of the build
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The short triggers
    /// </summary>
    public List<RocketSurgeonGitHubActionsTrigger> ShortTriggers { get; set; } = new();

    /// <summary>
    ///     The detailed triggers
    /// </summary>
    public List<GitHubActionsDetailedTrigger> DetailedTriggers { get; set; } = new();

    /// <summary>
    ///     The jobs
    /// </summary>
    public List<RocketSurgeonsGithubActionsJobBase> Jobs { get; set; } = new();

    /// <summary>
    ///     The dependencies of this job
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"name: {Name}");
        writer.WriteLine();

        if (ShortTriggers.Count > 0)
        {
            writer.WriteLine($"on: [{ShortTriggers.Select(x => x.GetValue().ToLowerInvariant()).JoinComma()}]");
        }
        else
        {
            writer.WriteLine("on:");
            using (writer.Indent())
            {
                DetailedTriggers.ForEach(x => x.Write(writer));
            }
        }

        writer.WriteKeyValues("env", Environment);

        writer.WriteLine();

        writer.WriteLine("jobs:");
        using (writer.Indent())
        {
            Jobs.ForEach(x => x.Write(writer));
        }
    }
}

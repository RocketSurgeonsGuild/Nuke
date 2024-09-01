using System.Collections.Immutable;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

#pragma warning disable CA1819
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A detailed trigger for version control
/// </summary>
public class RocketSurgeonGitHubActionsVcsTrigger : GitHubActionsDetailedTrigger
{
    /// <summary>
    ///     The kind of the trigger
    /// </summary>
    public RocketSurgeonGitHubActionsTrigger Kind { get; set; }

    /// <summary>
    ///     The branches
    /// </summary>
    public ImmutableArray<string> Branches { get; set; } = [];

    /// <summary>
    ///     The Tags
    /// </summary>
    public ImmutableArray<string> Tags { get; set; } = [];

    /// <summary>
    ///     The included paths
    /// </summary>
    public ImmutableArray<string> IncludePaths { get; set; } = [];

    /// <summary>
    ///     The excluded paths
    /// </summary>
    public ImmutableArray<string> ExcludePaths { get; set; } = [];

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine(Kind.GetValue() + ":");

        if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowDispatch or RocketSurgeonGitHubActionsTrigger.WorkflowCall) return;
        using (writer.Indent())
        {
            if (Branches.Length > 0)
            {
                writer.WriteLine("branches:");
                using (writer.Indent())
                {
                    Branches.ForEach(x => writer.WriteLine($"- '{x}'"));
                }
            }

            if (Tags.Length > 0)
            {
                writer.WriteLine("tags:");
                using (writer.Indent())
                {
                    Tags.ForEach(x => writer.WriteLine($"- '{x}'"));
                }
            }

            if (IncludePaths.Length == 0 && ExcludePaths.Length > 0)
            {
                writer.WriteLine("paths-ignore:");
                using (writer.Indent())
                {
                    ExcludePaths.ForEach(x => writer.WriteLine($"- '{x}'"));
                }
            }
            else if (IncludePaths.Length > 0 && ExcludePaths.Length == 0)
            {
                writer.WriteLine("paths:");
                using (writer.Indent())
                {
                    IncludePaths.ForEach(x => writer.WriteLine($"- '{x}'"));
                }
            }
            else if (IncludePaths.Length > 0 || ExcludePaths.Length > 0)
            {
                writer.WriteLine("paths:");
                using (writer.Indent())
                {
                    IncludePaths.ForEach(x => writer.WriteLine($"- '{x}'"));
                    ExcludePaths.ForEach(x => writer.WriteLine($"- '!{x}'"));
                }
            }
        }
    }
}

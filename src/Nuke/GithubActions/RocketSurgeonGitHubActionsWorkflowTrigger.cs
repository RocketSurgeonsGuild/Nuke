using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A detailed trigger for version control
/// </summary>
public class RocketSurgeonGitHubActionsWorkflowTrigger : GitHubActionsDetailedTrigger
{
    /// <summary>
    ///     The kind of the trigger
    /// </summary>
    public RocketSurgeonGitHubActionsTrigger Kind { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine(Kind.GetValue() + ":");

        if (Kind is not RocketSurgeonGitHubActionsTrigger.WorkflowDispatch and not RocketSurgeonGitHubActionsTrigger.WorkflowCall) return;
        using (writer.Indent())
        {
            if (Inputs.Any())
            {
                writer.WriteLine("inputs:");
                using (writer.Indent())
                {
                    foreach (var input in Inputs)
                    {
                        writer.WriteLine($"{input.Name}:");
                        using (writer.Indent())
                        {
                            writer.WriteLine($"type: {input.Type.GetValue()}");
                            if (!string.IsNullOrWhiteSpace(input.Description))
                            {
                                writer.WriteLine($"description: '{input.Description}'");
                            }

                            writer.WriteLine($"required: {input.Required ?? false}");

                            if (input.Default != null)
                            {
                                writer.WriteLine($"default: {input.Default}");
                            }
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowCall && Secrets.Any())
            {
                writer.WriteLine("secrets:");
                using (writer.Indent())
                {
                    foreach (var input in Secrets)
                    {
                        writer.WriteLine($"{input.Name}:");
                        using (writer.Indent())
                        {
                            if (!string.IsNullOrWhiteSpace(input.Description))
                            {
                                writer.WriteLine($"description: '{input.Description}'");
                            }

                            writer.WriteLine($"required: {input.Required ?? false}");
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowCall && Outputs.Any())
            {
                writer.WriteLine("ouputs:");
                using (writer.Indent())
                {
                    foreach (var input in Outputs)
                    {
                        writer.WriteLine($"{input.Name}:");
                        using (writer.Indent())
                        {
                            if (!string.IsNullOrWhiteSpace(input.Description))
                            {
                                writer.WriteLine($"description: '{input.Description}'");
                            }

                            writer.WriteLine($"value: {input.Value}");
                        }
                    }
                }
            }
        }
    }

    public List<GitHubActionsInput> Inputs { get; set; } = new();
    public List<GitHubActionsSecret> Secrets { get; set; } = new();
    public List<GitHubActionsOutput> Outputs { get; set; } = new();
}

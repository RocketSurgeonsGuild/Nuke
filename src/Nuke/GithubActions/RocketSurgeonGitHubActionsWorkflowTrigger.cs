using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;

#pragma warning disable CA1002, CA2227
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

    /// <summary>
    ///     The input variables for the workflow
    /// </summary>
    public List<GitHubActionsInput> Inputs { get; set; } = new();

    /// <summary>
    ///     The secret variables for the workflow
    /// </summary>
    public List<GitHubActionsSecret> Secrets { get; set; } = new();

    /// <summary>
    ///     The output variables for the workflow
    /// </summary>
    public List<GitHubActionsWorkflowOutput> Outputs { get; set; } = new();

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine(Kind.GetValue() + ":");

        if (Kind is not RocketSurgeonGitHubActionsTrigger.WorkflowDispatch and not RocketSurgeonGitHubActionsTrigger.WorkflowCall) return;
        using (writer.Indent())
        {
            if (Inputs.Count > 0)
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
                            if (!string.IsNullOrWhiteSpace(input.Description)) writer.WriteLine($"description: '{input.Description}'");

                            writer.WriteLine($"required: {input.Required ?? false}");

                            if (input.Default != null) writer.WriteLine($"default: {input.Default}");
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowCall && Secrets.Count > 0)
            {
                writer.WriteLine("secrets:");
                using (writer.Indent())
                {
                    foreach (var input in Secrets)
                    {
                        writer.WriteLine($"{input.Name}:");
                        using (writer.Indent())
                        {
                            if (!string.IsNullOrWhiteSpace(input.Description)) writer.WriteLine($"description: '{input.Description}'");

                            writer.WriteLine($"required: {input.Required ?? false}");
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowCall && Outputs.Count > 0)
            {
                writer.WriteLine("outputs:");
                using (writer.Indent())
                {
                    foreach (var input in Outputs)
                    {
                        writer.WriteLine($"{input.OutputName}:");
                        using (writer.Indent())
                        {
                            if (!string.IsNullOrWhiteSpace(input.Description)) writer.WriteLine($"description: '{input.Description}'");

                            writer.WriteLine($"value: {input}");
                        }
                    }
                }
            }
        }
    }
}

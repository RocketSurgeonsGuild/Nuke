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
    public List<GitHubActionsInput> Inputs { get; set; } = [];

    /// <summary>
    ///     The secret variables for the workflow
    /// </summary>
    public List<GitHubActionsSecret> Secrets { get; set; } = [];

    /// <summary>
    ///     The output variables for the workflow
    /// </summary>
    public List<GitHubActionsWorkflowOutput> Outputs { get; set; } = [];

    /// <summary>
    /// The types
    /// </summary>
    public List<string> Types { get; set; } = [];

    /// <summary>
    /// The workflows
    /// </summary>
    public List<string> Workflows { get; set; } = [];

    /// <summary>
    /// The branches
    /// </summary>
    public List<string> Branches { get; set; } = [];



    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine(Kind.GetValue() + ":");

        if (Kind is not RocketSurgeonGitHubActionsTrigger.WorkflowDispatch
                and not RocketSurgeonGitHubActionsTrigger.WorkflowCall
                and not RocketSurgeonGitHubActionsTrigger.WorkflowRun)
        {
            return;
        }

        using (writer.Indent())
        {
            if (Kind is not RocketSurgeonGitHubActionsTrigger.WorkflowRun)
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
                                if (!string.IsNullOrWhiteSpace(input.Description))
                                {
                                    writer.WriteLine($"description: '{input.Description}'");
                                }

                                writer.WriteLine($"required: {( input.Required ?? false ).ToString().ToLowerInvariant()}");

                                if (input.Default is not null)
                                {
                                    writer.WriteLine($"default: {input.Default}");
                                }
                            }
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowRun)
            {
                if (Workflows.Count > 0)
                {
                    writer.WriteLine("workflows:");
                    using (writer.Indent())
                    {
                        foreach (var input in Workflows)
                        {
                            writer.WriteLine($"- '{input}'");
                        }
                    }
                }

                if (Types.Count > 0)
                {
                    writer.WriteLine("types:");
                    using (writer.Indent())
                    {
                        foreach (var input in Types)
                        {
                            writer.WriteLine($"- '{input}'");
                        }
                    }
                }

                if (Branches.Count > 0)
                {
                    writer.WriteLine("branches:");
                    using (writer.Indent())
                    {
                        foreach (var input in Branches)
                        {
                            writer.WriteLine($"- '{input}'");
                        }
                    }
                }
            }

            if (Kind is RocketSurgeonGitHubActionsTrigger.WorkflowCall)
            {
                if (Secrets.Count > 0)
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

                                writer.WriteLine($"required: {( input.Required ?? false ).ToString().ToLowerInvariant()}");
                            }
                        }
                    }
                }

                if (Outputs.Count > 0)
                {
                    writer.WriteLine("outputs:");
                    using (writer.Indent())
                    {
                        foreach (var input in Outputs)
                        {
                            writer.WriteLine($"{input.OutputName}:");
                            using (writer.Indent())
                            {
                                if (!string.IsNullOrWhiteSpace(input.Description))
                                {
                                    writer.WriteLine($"description: '{input.Description}'");
                                }

                                writer.WriteLine($"value: {input}");
                            }
                        }
                    }
                }
            }
        }
    }
}

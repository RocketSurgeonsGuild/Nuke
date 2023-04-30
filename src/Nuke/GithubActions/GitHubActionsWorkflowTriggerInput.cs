namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsWorkflowTriggerInput(
    string Name,
    GitHubActionsWorkflowTriggerInputType Type = GitHubActionsWorkflowTriggerInputType.String,
    object? Default = null,
    string? Description = null,
    bool? Required = null,
    string? Alias = null
) : ITriggerValue
{
    public string Prefix => "inputs";
}

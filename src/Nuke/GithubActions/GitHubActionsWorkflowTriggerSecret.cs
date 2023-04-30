namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsWorkflowTriggerSecret(string Name, string? Description = null, bool? Required = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "secrets";
}

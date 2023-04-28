namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsWorkflowTriggerOutput(string Name, string? Description = null, string? Value = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "outputs";
}

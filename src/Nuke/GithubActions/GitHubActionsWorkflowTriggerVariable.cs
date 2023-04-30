namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsWorkflowTriggerVariable(string Name, string? Description = null, bool? Required = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "vars";
}

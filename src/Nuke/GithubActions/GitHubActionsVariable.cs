namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsVariable(string Name, string? Description = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "vars";
    public string? Default => null;
}

namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsSecret(string Name, string? Description = null, bool? Required = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "secrets";
    public string? Default => null;
}

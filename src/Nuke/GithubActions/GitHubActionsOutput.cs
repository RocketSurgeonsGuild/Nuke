namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsOutput(string Name, string? Description = null, string? Value = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "outputs";
    public string? Default => null;
}

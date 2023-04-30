namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsEnvironmentVariable(string Name, string? Default = null, string? Alias = null) : ITriggerValue
{
    public string Prefix => "env";
    public string? Description => null;
}

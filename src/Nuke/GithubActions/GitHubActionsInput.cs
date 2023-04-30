namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsInput(
    string Name,
    GitHubActionsInputType Type = GitHubActionsInputType.String,
    string? Default = null,
    string? Description = null,
    bool? Required = null,
    string? Alias = null
) : ITriggerValue
{
    public string Prefix => "inputs";
}

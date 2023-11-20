namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions input variable
/// </summary>
/// <param name="Name"></param>
/// <param name="Type"></param>
/// <param name="Default"></param>
/// <param name="Description"></param>
/// <param name="Required"></param>
/// <param name="Alias"></param>
public record GitHubActionsInput(
    string Name,
    GitHubActionsInputType Type = GitHubActionsInputType.String,
    string? Default = null,
    string? Description = null,
    bool? Required = null,
    string? Alias = null
) : ITriggerValue
{
    /// <inheritdoc />
    public string Prefix => "inputs";
}

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A github actions variable
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Alias"></param>
public record GitHubActionsVariable(string Name, string? Description = null, string? Alias = null) : ITriggerValue
{
    /// <inheritdoc />
    public string Prefix => "vars";

    /// <inheritdoc />
    public string? Default => null;
}

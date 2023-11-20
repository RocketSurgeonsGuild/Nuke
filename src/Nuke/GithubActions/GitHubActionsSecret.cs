namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Defines a github action secret
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Required"></param>
/// <param name="Alias"></param>
public record GitHubActionsSecret(string Name, string? Description = null, bool? Required = null, string? Alias = null) : ITriggerValue
{
    /// <inheritdoc />
    public string Prefix => "secrets";
    /// <inheritdoc />
    public string? Default => null;
}

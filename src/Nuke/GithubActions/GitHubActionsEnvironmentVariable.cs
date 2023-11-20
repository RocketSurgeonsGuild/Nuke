namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A github actions environment variable
/// </summary>
/// <param name="Name"></param>
/// <param name="Default"></param>
/// <param name="Alias"></param>
public record GitHubActionsEnvironmentVariable(string Name, string? Default = null, string? Alias = null) : ITriggerValue
{
    /// <inheritdoc />
    public string Prefix => "env";

    /// <inheritdoc />
    public string? Description => null;
}

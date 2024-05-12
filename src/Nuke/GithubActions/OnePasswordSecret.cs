namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github action 1Password based secret
/// </summary>
/// <param name="Path">The second half the op reference path</param>
/// <param name="Name">The name of the variable to be output</param>
/// <param name="Description"></param>
/// <param name="Alias">An alias for use with parameter attributes</param>
/// <param name="Variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
/// <param name="Secret">The secret where the OP_SERVICE_ACCOUNT_TOKEN is stored (defaults to OP_SERVICE_ACCOUNT_TOKEN)</param>
public record OnePasswordSecret
(
    string Path,
    string Name,
    string? Description = null,
    string? Alias = null,
    string? Variable = null,
    string? Secret = null) : ITriggerValue
{
    /// <inheritdoc />
    public string Prefix => "steps.1password.outputs";

    /// <inheritdoc />
    public string? Default => null;
}

using System.Security.Cryptography;
using System.Text;

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
public record OnePasswordServiceAccountSecret
(
    string Path,
    string Name,
    string? Description = null,
    string? Alias = null,
    string? Variable = null,
    string Secret = "OP_SERVICE_ACCOUNT_TOKEN") : ITriggerValue
{
    /// <inheritdoc />
    public string? Default => null;

    /// <inheritdoc />
    public string Prefix => $"steps.{OutputId}.outputs";

    internal string OutputId => $"op{HashId(Secret)}";

    private static string HashId(string value)
    {
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        // data to a set of hex strings
        var sBuilder = new StringBuilder();
        foreach (var t in data)
        {
            sBuilder.Append(t.ToString("x2"));
        }

        return sBuilder.ToString()[..8];
    }
}

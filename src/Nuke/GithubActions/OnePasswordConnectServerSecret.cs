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
/// <param name="ConnectHost">The Connect Server Host (defaults to OP_CONNECT_HOST)</param>
/// <param name="ConnectToken">The Connect Server Token (defaults to OP_CONNECT_TOKEN)</param>
public sealed record OnePasswordConnectServerSecret
(
    string Path,
    string Name,
    string? Description = null,
    string? Alias = null,
    string? Variable = null,
    string ConnectHost = "OP_CONNECT_HOST",
    string ConnectToken = "OP_CONNECT_TOKEN") : ITriggerValue
{
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

    internal string OutputId => $"op{HashId(ConnectHost + ConnectToken)}";

    /// <inheritdoc />
    public string Prefix => $"steps.{OutputId}.outputs";

    /// <inheritdoc />
    public string? Default => null;
}

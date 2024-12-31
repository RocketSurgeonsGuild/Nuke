namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A one password connect server secret
/// </summary>
/// <remarks>
///     The constructor for the <see cref="OnePasswordConnectServerSecretAttribute" />
/// </remarks>
/// <param name="name">The name of the variable to be output</param>
/// <param name="path">The reference path to the item</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OnePasswordConnectServerSecretAttribute(string name, string path) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     The constructor for the <see cref="OnePasswordConnectServerSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
    /// <param name="path">The second half the op reference path</param>
    /// param>
    public OnePasswordConnectServerSecretAttribute(string name, string variable, string path) : this(name, path) => Variable = variable;

    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    public OnePasswordConnectServerSecret ToSecret() =>
        new(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            ConnectHost ?? "OP_CONNECT_HOST",
            ConnectToken ?? "OP_CONNECT_TOKEN"
        );

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToSecret();

    /// <summary>
    ///     The value for the connect host (defaults to ${{ vars.OP_CONNECT_HOST }})
    /// </summary>
    public string? ConnectHost
    {
        get;
        [UsedImplicitly]
        set;
    }

    /// <summary>
    ///     The value for the connect token (defaults to ${{ secrets.OP_CONNECT_TOKEN }})
    /// </summary>
    public string? ConnectToken
    {
        get;
        [UsedImplicitly]
        set;
    }

    /// <summary>
    ///     The path to the item
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    ///     The github variable that defines the item in the vault
    /// </summary>
    public string? Variable { get; }
}

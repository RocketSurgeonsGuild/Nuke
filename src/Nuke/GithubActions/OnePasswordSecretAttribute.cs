namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A one password secret
/// </summary>
/// <remarks>
///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
/// </remarks>
/// <param name="name">The name of the variable to be output</param>
/// <param name="path">The reference path to the item</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OnePasswordSecretAttribute(string name, string path) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
    /// <param name="path">The second half the op reference path</param>
    /// param>
    public OnePasswordSecretAttribute(string name, string variable, string path) : this(name, path) => Variable = variable;

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToSecret();

    /// <summary>
    ///     The value for the connect host (defaults to ${{ vars.OP_CONNECT_HOST }})
    /// </summary>
    public string? ConnectHost
    {
        get;
        set
        {
            UseConnectServer = true;
            field = value;
        }
    }

    /// <summary>
    ///     The value for the connect token (defaults to ${{ secrets.OP_CONNECT_TOKEN }})
    /// </summary>
    public string? ConnectToken
    {
        get;
        set
        {
            UseConnectServer = true;
            field = value;
        }
    }

    /// <summary>
    ///     The path to the item
    /// </summary>
    public string Path { get; } = path;

    /// <summary>
    ///     The secret where the OP_SERVICE_ACCOUNT_TOKEN is stored (defaults to OP_SERVICE_ACCOUNT_TOKEN)
    /// </summary>
    public string? Secret
    {
        get;
        set
        {
            UseServiceAccount = true;
            field = value;
        }
    }

    /// <summary>
    ///     If you are using a service account for connect server
    /// </summary>
    public bool UseConnectServer
    {
        get => !UseServiceAccount;
        set => UseServiceAccount = !value;
    }

    /// <summary>
    ///     If you are using a service account for connect server
    /// </summary>
    public bool UseServiceAccount { get; set; } = true;

    /// <summary>
    ///     The github variable that defines the item in the vault
    /// </summary>
    public string? Variable { get; }

    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    internal ITriggerValue ToSecret() => UseConnectServer
        ? new OnePasswordConnectServerSecret(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            ConnectHost ?? "OP_CONNECT_HOST",
            ConnectToken ?? "OP_CONNECT_TOKEN"
        )
        : new OnePasswordServiceAccountSecret(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            Secret ?? "OP_SERVICE_ACCOUNT_TOKEN"
        );
}

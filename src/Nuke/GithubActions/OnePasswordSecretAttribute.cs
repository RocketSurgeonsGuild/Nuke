namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OnePasswordSecretAttribute : TriggerValueAttribute
{
    private string? connectToken;
    private string? connectHost;
    private string? secret;

    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="path">The reference path to the item</param>
    public OnePasswordSecretAttribute(string name, string path) : base(name)
    {
        Path = path;
    }

    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
    /// <param name="path">The second half the op reference path</param>
    /// param>
    public OnePasswordSecretAttribute(string name, string variable, string path) : this(name, path)
    {
        Variable = variable;
    }

    /// <summary>
    ///     If you are using a service account for connect server
    /// </summary>
    public bool UseServiceAccount { get; set; } = true;

    /// <summary>
    ///     If you are using a service account for connect server
    /// </summary>
    public bool UseConnectServer
    {
        get => !UseServiceAccount;
        set => UseServiceAccount = !value;
    }

    /// <summary>
    ///     The github variable that defines the item in the vault
    /// </summary>
    public string? Variable { get; }

    /// <summary>
    ///     The path to the item
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     The secret where the OP_SERVICE_ACCOUNT_TOKEN is stored (defaults to OP_SERVICE_ACCOUNT_TOKEN)
    /// </summary>
    public string? Secret
    {
        get => secret;
        set
        {
            UseServiceAccount = true;
            secret = value;
        }
    }

    /// <summary>
    ///     The value for the connect host (defaults to ${{ vars.OP_CONNECT_HOST }})
    /// </summary>
    public string? ConnectHost
    {
        get => connectHost;
        set
        {
            UseConnectServer = true;
            connectHost = value;
        }
    }

    /// <summary>
    ///     The value for the connect token (defaults to ${{ secrets.OP_CONNECT_TOKEN }})
    /// </summary>
    public string? ConnectToken
    {
        get => connectToken;
        set
        {
            UseConnectServer = true;
            connectToken = value;
        }
    }

    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    internal ITriggerValue ToSecret()
    {
        if (UseConnectServer)
        {
            return new OnePasswordConnectServerSecret(
                Path,
                Name,
                Description,
                Alias,
                Variable,
                ConnectHost ?? "OP_CONNECT_HOST",
                ConnectToken ?? "OP_CONNECT_TOKEN"
            );
        }

        return new OnePasswordServiceAccountSecret(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            Secret ?? "OP_SERVICE_ACCOUNT_TOKEN"
        );
    }

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue()
    {
        return ToSecret();
    }
}
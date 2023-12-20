namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OnePasswordConnectServerSecretAttribute : TriggerValueAttribute
{
    /// <summary>
    ///     The constructor for the <see cref="OnePasswordConnectServerSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="path">The reference path to the item</param>
    public OnePasswordConnectServerSecretAttribute(string name, string path) : base(name)
    {
        Path = path;
    }

    /// <summary>
    ///     The constructor for the <see cref="OnePasswordConnectServerSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
    /// <param name="path">The second half the op reference path</param>
    /// param>
    public OnePasswordConnectServerSecretAttribute(string name, string variable, string path) : this(name, path)
    {
        Variable = variable;
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
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    public OnePasswordConnectServerSecret ToSecret()
    {
        return new(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            ConnectHost,
            ConnectToken
        );
    }
}
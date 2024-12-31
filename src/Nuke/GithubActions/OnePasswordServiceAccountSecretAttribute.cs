namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     one password service account secret
/// </summary>
/// <remarks>
///     The constructor for the <see cref="OnePasswordServiceAccountSecretAttribute" />
/// </remarks>
/// <param name="name">The name of the variable to be output</param>
/// <param name="path">The reference path to the item</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class OnePasswordServiceAccountSecretAttribute(string name, string path) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     The constructor for the <see cref="OnePasswordServiceAccountSecretAttribute" />
    /// </summary>
    /// <param name="name">The name of the variable to be output</param>
    /// <param name="variable">The GitHub variable to item path part for the op reference (eg. op://vault/item)</param>
    /// <param name="path">The second half the op reference path</param>
    /// param>
    public OnePasswordServiceAccountSecretAttribute(string name, string variable, string path) : this(name, path) => Variable = variable;

    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    public OnePasswordServiceAccountSecret ToSecret() =>
        new(
            Path,
            Name,
            Description,
            Alias,
            Variable,
            Secret ?? "OP_SERVICE_ACCOUNT_TOKEN"
        );

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToSecret();

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
        [UsedImplicitly]
        set;
    }

    /// <summary>
    ///     The github variable that defines the item in the vault
    /// </summary>
    public string? Variable { get; }
}

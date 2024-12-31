namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions secret variable
/// </summary>
/// <remarks>
///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
/// </remarks>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsSecretAttribute(string name) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    public GitHubActionsSecret ToSecret() => new(Name, Description, Required, Alias);

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToSecret();

    /// <summary>
    ///     Is the secret required
    /// </summary>
    public bool? Required { get; set; }
}

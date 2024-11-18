namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions secret variable
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsSecretAttribute : TriggerValueAttribute
{
    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsSecretAttribute" />
    /// </summary>
    /// <param name="name"></param>
    public GitHubActionsSecretAttribute(string name) : base(name) { }

    /// <summary>
    ///     Is the secret required
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    ///     Convert to a secret
    /// </summary>
    /// <returns></returns>
    public GitHubActionsSecret ToSecret() => new(Name, Description, Required, Alias);

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToSecret();
}

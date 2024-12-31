namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions variable
/// </summary>
/// <remarks>
///     The constructor for the <see cref="GitHubActionsVariableAttribute" />
/// </remarks>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsVariableAttribute(string name) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     Is the variable required
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    ///     Convert to a variable
    /// </summary>
    /// <returns></returns>
    public GitHubActionsVariable ToVariable() => new(Name, Description, Alias);

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToVariable();
}

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Defines a github actions variable
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsVariableAttribute : TriggerValueAttribute
{
    /// <summary>
    /// Is the variable required
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// The constructor for the <see cref="GitHubActionsVariableAttribute"/>
    /// </summary>
    /// <param name="name"></param>
    public GitHubActionsVariableAttribute(string name) : base(name)
    {
    }

    /// <summary>
    /// Convert to a variable
    /// </summary>
    /// <returns></returns>
    public GitHubActionsVariable ToVariable()
    {
        return new GitHubActionsVariable(Name, Description, Alias);
    }
}

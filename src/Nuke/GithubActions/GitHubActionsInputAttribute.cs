namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Defines a github actions input variable
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsInputAttribute : TriggerValueAttribute
{
    /// <summary>
    /// The type of the input
    /// </summary>
    public GitHubActionsInputType Type { get; set; } = GitHubActionsInputType.String;

    /// <summary>
    /// Is this input required
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    /// The constructor for the <see cref="GitHubActionsInputAttribute"/>
    /// </summary>
    /// <param name="name"></param>
    public GitHubActionsInputAttribute(string name) : base(name)
    {
    }

    /// <summary>
    /// Convert the attribute into an input
    /// </summary>
    /// <returns></returns>
    public GitHubActionsInput ToInput()
    {
        return new GitHubActionsInput(Name, Type, Default, Description, Required, Alias);
    }
}

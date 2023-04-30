namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GitHubActionsInputAttribute : TriggerValueAttribute
{
    public GitHubActionsInputType Type { get; set; } = GitHubActionsInputType.String;
    public string? Default { get; set; }
    public bool? Required { get; set; }

    public GitHubActionsInputAttribute(string name) : base(name)
    {
    }

    public GitHubActionsInput ToInput()
    {
        return new GitHubActionsInput(Name, Type, Default, Description, Required, Alias);
    }
}

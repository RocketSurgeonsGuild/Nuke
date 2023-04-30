namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsVariableAttribute : TriggerValueAttribute
{
    public bool? Required { get; set; }

    public GitHubActionsVariableAttribute(string name) : base(name)
    {
    }

    public GitHubActionsVariable ToVariable()
    {
        return new GitHubActionsVariable(Name, Description, Alias ?? Name.Humanize());
    }
}

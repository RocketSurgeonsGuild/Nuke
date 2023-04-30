namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsOutputAttribute : TriggerValueAttribute
{
    public string? Value { get; set; }

    public GitHubActionsOutputAttribute(string name) : base(name)
    {
    }

    public GitHubActionsOutput ToOutput()
    {
        return new GitHubActionsOutput(Name, Description, Value, Alias);
    }
}

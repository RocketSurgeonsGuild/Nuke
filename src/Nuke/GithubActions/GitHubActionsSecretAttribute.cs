namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class)]
public sealed class GitHubActionsSecretAttribute : TriggerValueAttribute
{
    public bool? Required { get; set; }

    public GitHubActionsSecretAttribute(string name) : base(name)
    {
    }

    public GitHubActionsSecret ToSecret()
    {
        return new GitHubActionsSecret(Name, Description, Required, Alias);
    }
}

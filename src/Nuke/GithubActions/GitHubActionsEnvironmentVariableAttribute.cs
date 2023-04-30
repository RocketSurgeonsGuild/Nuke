namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsEnvironmentVariableAttribute : Attribute
{
    public string Name { get; }
    public string? Alias { get; set; }
    public string? Default { get; set; }

    public GitHubActionsEnvironmentVariableAttribute(string name)
    {
        Name = name;
    }

    public GitHubActionsEnvironmentVariable ToEnvironmentVariable()
    {
        return new GitHubActionsEnvironmentVariable(Name, Default, Alias);
    }
}

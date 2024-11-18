namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     An attribute to define this build consumes a given environment variable
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsEnvironmentVariableAttribute : Attribute
{
    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsEnvironmentVariableAttribute" />
    /// </summary>
    /// <param name="name"></param>
    public GitHubActionsEnvironmentVariableAttribute(string name) => Name = name;

    /// <inheritdoc cref="ITriggerValue.Name" />
    public string Name { get; }

    /// <inheritdoc cref="ITriggerValue.Alias" />
    public string? Alias { get; set; }

    /// <inheritdoc cref="ITriggerValue.Default" />
    public string? Default { get; set; }

    /// <summary>
    ///     Convert it to an environment variable
    /// </summary>
    /// <returns></returns>
    public GitHubActionsEnvironmentVariable ToEnvironmentVariable() => new(Name, Default, Alias);
}

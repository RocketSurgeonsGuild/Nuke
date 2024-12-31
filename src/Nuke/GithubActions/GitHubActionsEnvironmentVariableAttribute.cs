namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     An attribute to define this build consumes a given environment variable
/// </summary>
/// <remarks>
///     The constructor for the <see cref="GitHubActionsEnvironmentVariableAttribute" />
/// </remarks>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsEnvironmentVariableAttribute(string name) : Attribute
{
    /// <summary>
    ///     Convert it to an environment variable
    /// </summary>
    /// <returns></returns>
    public GitHubActionsEnvironmentVariable ToEnvironmentVariable() => new(Name, Default, Alias);

    /// <inheritdoc cref="ITriggerValue.Alias" />
    public string? Alias { get; set; }

    /// <inheritdoc cref="ITriggerValue.Default" />
    public string? Default { get; set; }

    /// <inheritdoc cref="ITriggerValue.Name" />
    public string Name { get; } = name;
}

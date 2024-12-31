namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions input variable
/// </summary>
/// <remarks>
///     The constructor for the <see cref="GitHubActionsInputAttribute" />
/// </remarks>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsInputAttribute(string name) : TriggerValueAttribute(name)
{
    /// <summary>
    ///     Convert the attribute into an input
    /// </summary>
    /// <returns></returns>
    public GitHubActionsInput ToInput() =>
        new(
            Name,
            Type,
            Default,
            Description,
            Required,
            Alias
        );

    /// <inheritdoc />
    public override ITriggerValue ToTriggerValue() => ToInput();

    /// <summary>
    ///     Is this input required
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    ///     The type of the input
    /// </summary>
    public GitHubActionsInputType Type { get; set; } = GitHubActionsInputType.String;
}

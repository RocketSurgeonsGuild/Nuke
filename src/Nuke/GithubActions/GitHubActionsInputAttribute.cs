namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions input variable
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class GitHubActionsInputAttribute : TriggerValueAttribute
{
    /// <summary>
    ///     The constructor for the <see cref="GitHubActionsInputAttribute" />
    /// </summary>
    /// <param name="name"></param>
    public GitHubActionsInputAttribute(string name) : base(name) { }

    /// <summary>
    ///     The type of the input
    /// </summary>
    public GitHubActionsInputType Type { get; set; } = GitHubActionsInputType.String;

    /// <summary>
    ///     Is this input required
    /// </summary>
    public bool? Required { get; set; }

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
}

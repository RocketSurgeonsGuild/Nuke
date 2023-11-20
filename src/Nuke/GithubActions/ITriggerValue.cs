namespace Rocket.Surgery.Nuke.GithubActions;

#pragma warning disable CA1716
/// <summary>
///     A trigger value for use with GithubActions
/// </summary>
public interface ITriggerValue
{
    /// <summary>
    ///     Name of the trigger value
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The alias for the trigger value if provided
    /// </summary>
    string? Alias { get; }

    /// <summary>
    ///     The description for the trigger value if provided
    /// </summary>
    string? Description { get; }

    /// <summary>
    ///     The prefix for the trigger value
    /// </summary>
    string Prefix { get; }

    /// <summary>
    ///     The default value for the trigger value if provided
    /// </summary>
    string? Default { get; }
}

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A trigger value attribute for use with GithubActions
/// </summary>
/// <remarks>
///     The default constructor
/// </remarks>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Class)]
public abstract class TriggerValueAttribute(string name) : Attribute
{
    /// <summary>
    ///     The name of the trigger value
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    ///     The alias for the trigger value if provided
    /// </summary>
    public string? Alias { get; set; }

    /// <summary>
    ///     The description for the trigger value if provided
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The default value for the trigger value if provided
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    ///     An internal way to get access to the trigger value
    /// </summary>
    /// <returns></returns>
    public abstract ITriggerValue ToTriggerValue();
}

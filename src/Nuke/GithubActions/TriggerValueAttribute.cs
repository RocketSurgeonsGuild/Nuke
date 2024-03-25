namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A trigger value attribute for use with GithubActions
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public abstract class TriggerValueAttribute : Attribute
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    protected TriggerValueAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     The name of the trigger value
    /// </summary>
    public string Name { get; }

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
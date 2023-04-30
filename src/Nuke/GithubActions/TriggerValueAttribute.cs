namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Class)]
public abstract class TriggerValueAttribute : Attribute
{
    public string Name { get; }
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? Default { get; set; }

    protected TriggerValueAttribute(string name)
    {
        Name = name;
    }
}

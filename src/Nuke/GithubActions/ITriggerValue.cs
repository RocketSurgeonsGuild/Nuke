namespace Rocket.Surgery.Nuke.GithubActions;

public interface ITriggerValue
{
    string Name { get; }
    string? Alias { get; }
    string? Description { get; }
    string Prefix { get; }
}

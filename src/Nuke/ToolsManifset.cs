namespace Rocket.Surgery.Nuke;

internal class ToolsManifset
{
    public Dictionary<string, ToolDefinition> Tools { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
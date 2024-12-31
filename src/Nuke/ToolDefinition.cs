namespace Rocket.Surgery.Nuke;

/// <summary>
///     A tool definition
/// </summary>
public class ToolDefinition
{
    /// <summary>
    ///     The commands
    /// </summary>
    public string[] Commands { get; set; } = [];

    /// <summary>
    ///     The version
    /// </summary>
    public string Version { get; set; } = null!;

    // ReSharper disable once NullableWarningSuppressionIsUsed
}

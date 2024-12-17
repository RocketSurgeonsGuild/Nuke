namespace Rocket.Surgery.Nuke;

/// <summary>
/// A tool definition
/// </summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ToolDefinition
{
    /// <summary>
    /// The version
    /// </summary>
    public string Version { get; set; } = null!;
    /// <summary>
    /// The commands
    /// </summary>
    public string[] Commands { get; set; } = [];

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
    // ReSharper disable once NullableWarningSuppressionIsUsed
}

namespace Rocket.Surgery.Nuke;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public record FullToolCommandDefinition
(
    string PackageId,
    string Version,
    string Command
)
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
}
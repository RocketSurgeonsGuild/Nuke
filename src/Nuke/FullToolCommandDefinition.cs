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

/* Unmerged change from project 'Rocket.Surgery.Nuke(net9.0)'
Before:
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }
After:
    private string DebuggerDisplay => ToString();
*/
    private string DebuggerDisplay => ToString();
}

using System.Diagnostics;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A wrapper around the SetupXvfb step in order to run commands in headless mode
/// </summary>
[PublicAPI]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class HeadlessRunStep : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public HeadlessRunStep(string name) : base(name) => Uses = "coactions/setup-xvfb@v1";

    // ReSharper disable once NullableWarningSuppressionIsUsed
    /// <summary>The script to run</summary>
    public string Run { get; set; } = null!;

    /// <summary>
    ///     The working directory where the script is run
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    ///     Options to pass to the xvfb server
    ///     See https://www.x.org/releases/current/doc/man/man1/Xvfb.1.xhtml#heading4 for the list of supported options
    /// </summary>
    public string? Options { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        WithProperties(x => x.Kebaberize());
        base.Write(writer);
    }
}

#pragma warning disable CA2225
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A enum like class that defines the possible shells in github actions
/// </summary>
[PublicAPI]
public class GithubActionShell
{
    /// <summary>
    ///     Get a shell from a given string
    /// </summary>
    /// <param name="shell"></param>
    /// <returns></returns>
    public static implicit operator GithubActionShell(string shell) => new(shell);

    /// <summary>
    ///     Convert a string into a shell
    /// </summary>
    /// <param name="shell"></param>
    /// <returns></returns>
    public static implicit operator string(GithubActionShell shell) => shell.Shell;

    /// <summary>
    ///     Bash shell
    /// </summary>
    public static GithubActionShell Bash { get; } = new("bash");

    /// <summary>
    ///     Powershell Core
    /// </summary>
    public static GithubActionShell Pwsh { get; } = new("pwsh");

    /// <summary>
    ///     Python
    /// </summary>
    public static GithubActionShell Python { get; } = new("python");

    /// <summary>
    ///     Shell
    /// </summary>
    public static GithubActionShell Sh { get; } = new("sh");

    /// <summary>
    ///     Windows Command Line
    /// </summary>
    public static GithubActionShell Cmd { get; } = new("cmd");

    /// <summary>
    ///     Windows Powershell
    /// </summary>
    public static GithubActionShell Powershell { get; } = new("powershell");

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="shell"></param>
    public GithubActionShell(string shell) => Shell = shell;

    /// <summary>
    ///     The shell
    /// </summary>
    public string Shell { get; }

    /// <inheritdoc />
    public override string ToString() => Shell;
}

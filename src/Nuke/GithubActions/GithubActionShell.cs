#pragma warning disable CA2225
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A enum like class that defines the possible shells in github actions
/// </summary>
/// <remarks>
///     The default constructor
/// </remarks>
/// <param name="shell"></param>
[PublicAPI]
public class GithubActionShell(string shell)
{
    /// <inheritdoc />
    public override string ToString() => Shell;

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
    ///     Windows Command Line
    /// </summary>
    public static GithubActionShell Cmd { get; } = new("cmd");

    /// <summary>
    ///     Windows Powershell
    /// </summary>
    public static GithubActionShell Powershell { get; } = new("powershell");

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
    ///     The shell
    /// </summary>
    public string Shell { get; } = shell;
}

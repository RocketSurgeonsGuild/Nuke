namespace Rocket.Surgery.Nuke;

/// <summary>
///     The trigger that the lint paths were created for
/// </summary>
public enum LintTrigger
{
    /// <summary>
    ///     The paths were created for a specified files
    /// </summary>
    Specific,

    /// <summary>
    ///     The local staged files
    /// </summary>
    Staged,

    /// <summary>
    ///     A pull request
    /// </summary>
    PullRequest,

    /// <summary>
    ///     No files.
    /// </summary>
    /// <remarks>
    ///     This could means tasks could be run on all files or no files, whatever is needed.
    /// </remarks>
    None,
}

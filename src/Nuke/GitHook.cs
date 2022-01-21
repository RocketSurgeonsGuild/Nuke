namespace Rocket.Surgery.Nuke;

/// <summary>
///     An interface that can be used to implement git hooks for your build
/// </summary>
public interface IGitHooksEngine
{
    /// <summary>
    ///     Determine if hooks are installed or not
    /// </summary>
    /// <param name="hooks"></param>
    /// <returns></returns>
    bool AreHooksInstalled(IReadOnlyCollection<string> hooks);

    /// <summary>
    ///     Install the given git hooks locally.
    /// </summary>
    /// <param name="hooks"></param>
    void InstallHooks(IReadOnlyCollection<string> hooks);
}

/// <summary>
///     The allows git hooks
/// </summary>
public enum GitHook
{
    /// <summary>
    ///     applypatch-msg
    /// </summary>
    ApplypatchMsg,

    /// <summary>
    ///     commit-msg
    /// </summary>
    CommitMsg,

    /// <summary>
    ///     post-applypatch
    /// </summary>
    PostApplypatch,

    /// <summary>
    ///     post-checkout
    /// </summary>
    PostCheckout,

    /// <summary>
    ///     post-commit
    /// </summary>
    PostCommit,

    /// <summary>
    ///     post-merge
    /// </summary>
    PostMerge,

    /// <summary>
    ///     post-receive
    /// </summary>
    PostReceive,

    /// <summary>
    ///     post-rewrite
    /// </summary>
    PostRewrite,

    /// <summary>
    ///     post-update
    /// </summary>
    PostUpdate,

    /// <summary>
    ///     pre-applypatch
    /// </summary>
    PreApplypatch,

    /// <summary>
    ///     pre-auto-gc
    /// </summary>
    PreAutoGc,

    /// <summary>
    ///     pre-commit
    /// </summary>
    PreCommit,

    /// <summary>
    ///     prepare-commit-msg
    /// </summary>
    PrepareCommitMsg,

    /// <summary>
    ///     pre-push
    /// </summary>
    PrePush,

    /// <summary>
    ///     pre-rebase
    /// </summary>
    PreRebase,

    /// <summary>
    ///     pre-receive
    /// </summary>
    PreReceive,

    /// <summary>
    ///     push-to-checkout
    /// </summary>
    PushToCheckout,

    /// <summary>
    ///     sendemail-validate
    /// </summary>
    SendemailValidate,

    /// <summary>
    ///     update
    /// </summary>
    Update
}

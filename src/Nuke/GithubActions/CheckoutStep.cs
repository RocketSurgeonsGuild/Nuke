namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The checkout action
/// </summary>
public class CheckoutStep : UsingStep
{
    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
#pragma warning disable CA1308
        WithProperties(x => x.Kebaberize());
#pragma warning restore CA1308
        base.Write(writer);
    }

    /// <summary>
    ///     Whether to execute `git clean -ffdx &amp;&amp; git reset --hard HEAD` before fetching
    ///     Default: true
    /// </summary>
    public bool Clean { get; set; }

    /// <summary>
    ///     Number of commits to fetch. 0 indicates all history.
    /// </summary>
    /// <remarks>Default: 1</remarks>
    public int? FetchDepth { get; set; }

    /// <summary>
    ///     Whether to download Git-LFS files
    /// </summary>
    /// <remarks>Default: false</remarks>
    public string? Lfs { get; set; }

    /// <summary>
    ///     Relative path under $GITHUB_WORKSPACE to place the repository
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     Whether to configure the token or SSH key with the local git config
    /// </summary>
    /// <remarks>Default: true</remarks>
    public bool? PersistCredentials { get; set; }

    /// <summary>
    ///     The branch, tag or SHA to checkout. When checking out the repository that
    ///     triggered a workflow, this defaults to the reference or SHA for that event.
    ///     Otherwise, defaults to `master`.
    /// </summary>
    /// <remarks>Default: master</remarks>
    public string? Ref { get; set; }

    /// <summary>
    ///     Repository name with owner. For example, actions/checkout
    /// </summary>
    /// <remarks>Default: ${{ github.repository }}</remarks>
    public string? Repository { get; set; }

    /// <summary>
    ///     <para>
    ///         SSH key used to fetch the repository. The SSH key is configured with the local
    ///         git config, which enables your scripts to run authenticated git commands. The
    ///         post-job step removes the SSH key.
    ///     </para>
    ///     <para>We recommend using a service account with the least permissions necessary.</para>
    ///     <para>
    ///         [Learn more about creating and using encrypted
    ///         secrets](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets)
    ///     </para>
    /// </summary>
    public string? SshKey { get; set; }

    /// <summary>
    ///     Known hosts in addition to the user and global host key database. The public SSH
    ///     keys for a host may be obtained using the utility `ssh-keyscan`. For example,
    ///     `ssh-keyscan github.com`. The public key for github.com is always implicitly
    ///     added.
    /// </summary>
    public string? SshKnownHosts { get; set; }

    /// <summary>
    ///     Whether to perform strict host key checking. When true, adds the options
    ///     `StrictHostKeyChecking=yes` and `CheckHostIP=no` to the SSH command line. Use
    ///     the input `ssh-known-hosts` to configure additional hosts.
    /// </summary>
    /// <remarks>Default: true</remarks>
    public bool? SshStrict { get; set; }

    /// <summary>
    ///     <para>
    ///         Whether to checkout submodules: `true` to checkout submodules or `recursive` to
    ///         recursively checkout submodules.
    ///     </para>
    ///     <para>
    ///         When the `ssh-key` input is not provided, SSH URLs beginning with
    ///         `git@github.com:` are converted to HTTPS.
    ///     </para>
    /// </summary>
    /// <remarks>Default: false</remarks>
    public string? Submodules { get; set; }

    /// <summary>
    ///     <para>
    ///         Personal access token (PAT) used to fetch the repository. The PAT is configured
    ///         with the local git config, which enables your scripts to run authenticated git
    ///         commands. The post-job step removes the PAT.
    ///     </para>
    ///     <para>
    ///         We recommend using a service account with the least permissions necessary. Also
    ///         when generating a new PAT, select the least scopes necessary.
    ///     </para>
    ///     <para>
    ///         [Learn more about creating and using encrypted
    ///         secrets](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets)
    ///     </para>
    /// </summary>
    /// <remarks>Default: ${{ github.token }}</remarks>
    public string? Token { get; set; }

    /// <summary>
    ///     Uses the checkout resource action
    /// </summary>
    /// <param name="name"></param>
    public CheckoutStep(string name) : base(name) => Uses = "actions/checkout@v4";
}

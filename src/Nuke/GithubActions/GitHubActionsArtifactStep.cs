using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Humanizer;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class UploadArtifactStep : UsingStep
    {
        public UploadArtifactStep(string name) : base(name)
        {
            Uses = "actions/upload-artifact@v1";
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            WithProperties(x => x.Underscore().Camelize().ToLowerInvariant());
            base.Write(writer);
        }

        protected override string GetStepName(string name)
        {
            return $"🏺 {name}";
        }
    }
    public class DownloadArtifactSet : UsingStep
    {
        public DownloadArtifactSet(string name) : base(name)
        {
            Uses = "actions/download-artifact@v1";
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            WithProperties(x => x.Underscore().Camelize().ToLowerInvariant());
            base.Write(writer);
        }

        protected override string GetStepName(string name)
        {
            return $"🚀 {name}";
        }
    }

    public class SetupDotNetStep : UsingStep
    {
        public SetupDotNetStep(string name) : base(name)
        {
            // temporary to deal with sxs issue roll back once https://github.com/actions/setup-dotnet/pull/71 is merged
            Uses = "actions/setup-dotnet@v1";
        }

        /// <summary>SDK version to use. Example: 2.2.104</summary>
        public string DotNetVersion { get; set; }

        /// <summary>
        /// Optional package source for which to set up authentication. Will consult any existing NuGet.config in the root of the repo and provide a temporary NuGet.config using the NUGET_AUTH_TOKEN environment variable as a ClearTextPassword
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// Optional OWNER for using packages from GitHub Package Registry organizations/users other than the current repository's owner. Only used if a GPR URL is also provided in source-url
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Optional NuGet.config location, if your NuGet.config isn't located in the root of the repo.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// The nuget auth token (NUGET_AUTH_TOKEN)
        /// </summary>
        /// <remarks>
        /// Defaults to ${{secrets.GITHUB_TOKEN}} if the source url is given
        /// </remarks>
        public string NuGetAuthToken { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            // WithProperties(x => x.Underscore().Dasherize().ToLowerInvariant());
            if (!string.IsNullOrWhiteSpace(DotNetVersion))
            {
                With.Add("dotnet-version", DotNetVersion);
            }
            if (!string.IsNullOrWhiteSpace(SourceUrl))
            {
                With.Add("source-url", SourceUrl);
            }
            if (!string.IsNullOrWhiteSpace(Owner))
            {
                With.Add("owner", Owner);
            }
            if (!string.IsNullOrWhiteSpace(SourceUrl))
            {
                NuGetAuthToken = string.IsNullOrWhiteSpace(NuGetAuthToken) ? "${{ secrets.GITHUB_TOKEN }}" : NuGetAuthToken;
            }
            if (!string.IsNullOrWhiteSpace(NuGetAuthToken))
            {
                Environment.Add("NUGET_AUTH_TOKEN", NuGetAuthToken);
            }
            base.Write(writer);
        }
    }

    public class CheckoutStep : UsingStep
    {
        public CheckoutStep(string name) : base(name)
        {
            Uses = "actions/checkout@v2";
        }

        /// <summary>
        /// Repository name with owner. For example, actions/checkout
        /// </summary>
        /// <remarks>Default: ${{ github.repository }}</remarks>
        public string Repository { get; set; }

        /// <summary>
        /// The branch, tag or SHA to checkout. When checking out the repository that
        /// triggered a workflow, this defaults to the reference or SHA for that event.
        /// Otherwise, defaults to `master`.
        /// </summary>
        /// <remarks>Default: master</remarks>
        public string Ref { get; set; }

        /// <summary>
        /// <para>
        /// Personal access token (PAT) used to fetch the repository. The PAT is configured
        /// with the local git config, which enables your scripts to run authenticated git
        /// commands. The post-job step removes the PAT.
        /// </para>
        /// <para>
        /// We recommend using a service account with the least permissions necessary. Also
        /// when generating a new PAT, select the least scopes necessary.
        /// </para>
        /// <para>[Learn more about creating and using encrypted secrets](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets)</para>
        ///
        /// </summary>
        /// <remarks>Default: ${{ github.token }}</remarks>
        public string Token { get; set; }

        /// <summary>
        /// <para>
        /// SSH key used to fetch the repository. The SSH key is configured with the local
        /// git config, which enables your scripts to run authenticated git commands. The
        /// post-job step removes the SSH key.
        /// </para>
        /// <para>We recommend using a service account with the least permissions necessary.</para>
        /// <para>[Learn more about creating and using encrypted secrets](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets)</para>
        /// </summary>
        public string SshKey { get; set; }

        /// <summary>
        /// Known hosts in addition to the user and global host key database. The public SSH
        /// keys for a host may be obtained using the utility `ssh-keyscan`. For example,
        /// `ssh-keyscan github.com`. The public key for github.com is always implicitly
        /// added.
        /// </summary>
        public string SshKnownHosts { get; set; }

        /// <summary>
        /// Whether to perform strict host key checking. When true, adds the options
        /// `StrictHostKeyChecking=yes` and `CheckHostIP=no` to the SSH command line. Use
        /// the input `ssh-known-hosts` to configure additional hosts.
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool? SshStrict { get; set; }

        /// <summary>
        /// Whether to configure the token or SSH key with the local git config
        /// </summary>
        /// <remarks>Default: true</remarks>
        public bool? PersistCredentials { get; set; }

        /// <summary>
        /// Relative path under $GITHUB_WORKSPACE to place the repository
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether to execute `git clean -ffdx &amp;&amp; git reset --hard HEAD` before fetching
        /// Default: true
        /// </summary>
        public bool Clean { get; set; }

        /// <summary>
        /// Number of commits to fetch. 0 indicates all history.
        /// </summary>
        /// <remarks>Default: 1</remarks>
        public int? FetchDepth { get; set; }

        /// <summary>
        /// Whether to download Git-LFS files
        /// </summary>
        /// <remarks>Default: false</remarks>
        public string Lfs { get; set; }

        /// <summary>
        /// <para>
        /// Whether to checkout submodules: `true` to checkout submodules or `recursive` to
        /// recursively checkout submodules.
        /// </para>
        /// <para>
        /// When the `ssh-key` input is not provided, SSH URLs beginning with
        /// `git@github.com:` are converted to HTTPS.
        /// </para>
        ///
        /// </summary>
        /// <remarks>Default: false</remarks>
        public string Submodules { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            WithProperties(x => x.Underscore().Hyphenate().ToLowerInvariant());
            base.Write(writer);
        }
    }
}
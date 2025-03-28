using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;

using Rocket.Surgery.Nuke.DotNetCore;

[PublicAPI]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
[EnsureGitHooks(GitHook.PreCommit)]
[DotNetVerbosityMapping]
[MSBuildVerbosityMapping]
[NuGetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
[LocalBuildConventions]
#pragma warning disable CA1050
internal partial class Pipeline : NukeBuild,
#pragma warning restore CA1050
    ICanRestoreWithDotNetCore,
    ICanBuildWithDotNetCore,
    ICanTestWithDotNetCore,
    ICanPackWithDotNetCore,
    ICanClean,
    IHaveCommonLintTargets,
    IRemoveUnusedDependencies,
    IHavePublicApis,
    IGenerateCodeCoverageReport,
    IGenerateCodeCoverageSummary,
    IGenerateCodeCoverageBadges,
    IHaveConfiguration<Configuration>
{
    /// <summary>
    ///     Support plugins are available for:
    ///     - JetBrains ReSharper        https://nuke.build/resharper
    ///     - JetBrains Rider            https://nuke.build/rider
    ///     - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///     - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Pipeline>(x => x.Default);

    public Target Build => _ => _;

    public Target Clean => _ => _;

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter]
    public string? GitHubToken { get; }

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    [GitVersion(NoFetch = true, NoCache = false)]
    public GitVersion GitVersion { get; } = null!;

    public Target Lint => _ => _;

    public Target Pack => _ => _;

    public Target Restore => _ => _;

    public Target Test => _ => _;

    [NonEntryTarget]
    private Target Default => _ => _
                                  .DependsOn(Restore)
                                  .DependsOn(Build)
                                  .DependsOn(Test)
                                  .DependsOn(Pack);

    [Solution(GenerateProjects = true)]
    private Solution Solution { get; } = null!;

    Nuke.Common.ProjectModel.Solution IHaveSolution.Solution => Solution;
}

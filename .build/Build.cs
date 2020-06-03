using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.DotNetCore;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AzurePipelinesSteps(
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[]
    {
        nameof(IHaveBuildVersion.BuildVersion), nameof(ITestWithDotNetCore.Trigger_Code_Coverage_Reports), nameof(Default)
    },
    ExcludedTargets = new[] { nameof(IRestoreWithDotNetCore.CoreRestore), nameof(IRestoreWithDotNetCore.DotnetToolRestore) },
    Parameters = new[]
    {
        nameof(ITestWithDotNetCore.CoverageDirectory), nameof(IOutputArtifacts.ArtifactsDirectory), nameof(Verbosity),
        nameof(Configuration)
    }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
[EnsureGitHooks(GitHook.PreCommit)]
internal class Solution : RocketBoosterBuild,
                          IRestoreWithDotNetCore,
                          IBuildWithDotNetCore,
                          ITestWithDotNetCore,
                          IPackWithDotNetCore,
                          IUseDataCollector,
                          IHaveBuildVersion,
                          ICanClean,
                          IGenerateCodeCoverageReport,
                          IGenerateCodeCoverageSummary,
                          IGenerateCodeCoverageBadges
{
    /// <summary>
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Solution>(x => x.Default);

    private Target Default => _ => _
       .DependsOn<IHaveBuildVersion>(x => x.BuildVersion)
       .DependsOn(Restore)
       .DependsOn(Build)
       .DependsOn(Test)
       .DependsOn(Pack);

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<IRestoreWithDotNetCore>(x => x.CoreRestore);
    public Target Build => _ => _.Inherit<IBuildWithDotNetCore>(x => x.CoreBuild);
    public Target Test => _ => _.Inherit<ITestWithDotNetCore>(x => x.CoreTest);
    public Target Pack => _ => _.Inherit<IPackWithDotNetCore>(x => x.CorePack)
       .DependsOn(Clean);

    [ComputedGitVersion]
    public GitVersion GitVersion { get; } = null!;

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    /// <summary>
    /// Configuration to build - Default is 'Debug' (local) or 'Release' (server)
    /// </summary>
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public string Configuration { get; } = null!;
}
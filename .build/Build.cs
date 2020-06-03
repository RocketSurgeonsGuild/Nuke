using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
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
        nameof(IHaveBuildVersion.BuildVersion), nameof(IDotNetCoreBuild.Trigger_Code_Coverage_Reports), nameof(Default)
    },
    ExcludedTargets = new[] { nameof(IDotNetCoreBuild.Restore), nameof(IDotNetCoreBuild.DotnetToolRestore) },
    Parameters = new[]
    {
        nameof(IDotNetCoreBuild.CoverageDirectory), nameof(IOutputArtifacts.ArtifactsDirectory), nameof(Verbosity),
        nameof(Configuration)
    }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
[EnsureGitHooks(GitHook.PreCommit)]
internal class Solution : RocketBoosterBuild,
                          IDotNetCoreBuild,
                          IUseDataCollector,
                          IPackWithDotNetCore,
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
       .DependsOn<IRestoreWithDotNetCore>(x => x.Restore)
       .DependsOn<IBuildWithDotNetCore>(x => x.Build)
       .DependsOn<ITestWithDotNetCore>(x => x.Test)
       .DependsOn<IPackWithDotNetCore>(x => x.Pack);

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<IRestoreWithDotNetCore>(x => x.Restore);
    public Target Build => _ => _.Inherit<IBuildWithDotNetCore>(x => x.Build);
    public Target Test => _ => _.Inherit<ITestWithDotNetCore>(x => x.Test);
    public Target Pack => _ => _.Inherit<IPackWithDotNetCore>(x => x.Pack);

    [ComputedGitVersion]
    public GitVersion GitVersion { get; }

    /// <summary>
    /// Configuration to build - Default is 'Debug' (local) or 'Release' (server)
    /// </summary>
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public string Configuration { get; }
}
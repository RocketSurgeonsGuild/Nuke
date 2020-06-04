using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.MsBuild;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AzurePipelinesSteps(
    InvokeTargets = new[] { nameof(Default) },
    NonEntryTargets = new[]
    {
        nameof(IHaveBuildVersion.BuildVersion), nameof(ICanTestWithDotNetCore.Trigger_Code_Coverage_Reports), nameof(Default)
    },
    ExcludedTargets = new[] { nameof(ICanRestoreWithDotNetCore.CoreRestore), nameof(ICanRestoreWithDotNetCore.DotnetToolRestore) },
    Parameters = new[]
    {
        nameof(IHaveCodeCoverage.CoverageDirectory), nameof(IHaveOutputArtifacts.ArtifactsDirectory), nameof(Verbosity), nameof(IHaveConfiguration. Configuration)
    }
)]
[PackageIcon(
    "https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png"
)]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
[EnsureGitHooks(GitHook.PreCommit)]
[DotNetVerbosityMapping]
[MSBuildVerbosityMapping]
[NuGetVerbosityMapping]
public class Solution : NukeBuild,
                          ICanRestoreWithDotNetCore,
                          ICanBuildWithDotNetCore,
                          ICanTestWithDotNetCore,
                          ICanPackWithDotNetCore,
                          ICanPackWithMsBuild,
                          IUseDataCollector,
                          IHaveBuildVersion,
                          ICanClean,
                          IGenerateCodeCoverageReport,
                          IGenerateCodeCoverageSummary,
                          IGenerateCodeCoverageBadges,
                          IHaveConfiguration<Configuration>
{
    [ComputedGitVersion]
    public GitVersion GitVersion { get; } = null!;

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    private Target Default => _ => _
       .DependsOn<IHaveBuildVersion>(x => x.BuildVersion)
       .DependsOn(Restore)
       .DependsOn(Build)
       .DependsOn(Test)
       .DependsOn(Pack);

    public Target BuildVersion => _ => _.Inherit<IHaveBuildVersion>(x => x.BuildVersion)
       .Before(Default)
       .Before(Clean);

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<ICanRestoreWithDotNetCore>(x => x.CoreRestore);
    public Target Build => _ => _.Inherit<ICanBuildWithDotNetCore>(x => x.CoreBuild);
    public Target Test => _ => _.Inherit<ICanTestWithDotNetCore>(x => x.CoreTest);
    public Target Pack => _ => _.Inherit<ICanPackWithDotNetCore>(x => x.CorePack)
       .DependsOn(Clean);
    
    /// <summary>
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Solution>(x => x.Default);
}
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.MsBuild;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[AzurePipelinesSteps(
    InvokeTargets = new[] { nameof(Default) },
    NonEntryTargets = new[]
    {
        nameof(ICIEnvironment.CIEnvironment),
        nameof(ITriggerCodeCoverageReports.Trigger_Code_Coverage_Reports),
        nameof(ITriggerCodeCoverageReports.Generate_Code_Coverage_Report_Cobertura),
        nameof(IGenerateCodeCoverageBadges.Generate_Code_Coverage_Badges),
        nameof(IGenerateCodeCoverageReport.Generate_Code_Coverage_Report),
        nameof(IGenerateCodeCoverageSummary.Generate_Code_Coverage_Summary),
        nameof(Default)
    },
    ExcludedTargets = new[]
        { nameof(ICanRestoreWithDotNetCore.Restore), nameof(ICanRestoreWithDotNetCore.DotnetToolRestore) },
    Parameters = new[]
    {
        nameof(IHaveCodeCoverage.CoverageDirectory), nameof(IHaveOutputArtifacts.ArtifactsDirectory), nameof(Verbosity),
        nameof(IHaveConfiguration.Configuration)
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
[PrintBuildVersion, PrintCIEnvironment]
public class Solution : NukeBuild,
                        ICanRestoreWithDotNetCore,
                        ICanBuildWithDotNetCore,
                        ICanTestWithDotNetCore,
                        ICanPackWithDotNetCore,
                        ICanPackWithMsBuild,
                        IUseDataCollector,
                        ICanClean,
                        IGenerateCodeCoverageReport,
                        IGenerateCodeCoverageSummary,
                        IGenerateCodeCoverageBadges,
                        IHaveConfiguration<Configuration>,
                        ICanLint
{
    /// <summary>
    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main() => Execute<Solution>(x => x.Default);

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    private Target Default => _ => _
       .DependsOn(Restore)
       .DependsOn(Build)
       .DependsOn(Test)
       .DependsOn(Pack);

    public Target Build => _ => _.Inherit<ICanBuildWithDotNetCore>(x => x.CoreBuild);

    public Target Pack => _ => _.Inherit<ICanPackWithDotNetCore>(x => x.CorePack)
       .DependsOn(Clean);

    [ComputedGitVersion]
    public GitVersion GitVersion { get; } = null!;

    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<ICanRestoreWithDotNetCore>(x => x.CoreRestore);
    public Target Test => _ => _.Inherit<ICanTestWithDotNetCore>(x => x.CoreTest);

    public Target BuildVersion => _ => _.Inherit<IHaveBuildVersion>(x => x.BuildVersion)
       .Before(Default)
       .Before(Clean);


    [CI] public AzurePipelines? AzurePipelines { get; }
    public Target CIPlayground => _ => _
       .DependentFor(Default)
       .OnlyWhenStatic(() => IsServerBuild)
       .Executes(async 
        () =>
        {
            var task = AzurePipelines?.CreateTask("mytype", "mytask", 1);
            for (var i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                AzurePipelines?.UpdateTask(task, i * 10, AzurePipelinesTaskState.InProgress);
                var childTask = AzurePipelines?.CreateTask("child", "mychildtask" + (i+1), i+1, parent: task);
                for (var j = 0; i < 10; i++)
                {
                    AzurePipelines?.UpdateTask(childTask, j * 10, AzurePipelinesTaskState.InProgress);
                    await Task.Delay(100);
                }
                AzurePipelines?.CompleteTask(childTask, AzurePipelinesTaskResult.Succeeded);
            }
            AzurePipelines?.CompleteTask(task, AzurePipelinesTaskResult.SucceededWithIssues);
        }
    );

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;
}
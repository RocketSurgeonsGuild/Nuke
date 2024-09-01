using JetBrains.Annotations;
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
public partial class Pipeline : NukeBuild,
    #pragma warning restore CA1050
    ICanRestoreWithDotNetCore,
    ICanBuildWithDotNetCore,
    ICanTestWithDotNetCore,
    ICanPackWithDotNetCore,
    IHaveDataCollector,
    ICanClean,
    ICanDotNetFormat,
    ICanPrettier,
    IHavePublicApis,
    ICanUpdateReadme,
    IGenerateCodeCoverageReport,
    IGenerateCodeCoverageSummary,
    IGenerateCodeCoverageBadges,
    ICanRegenerateBuildConfiguration,
    IHaveConfiguration<Configuration>
{
    /// <summary>
    ///     Support plugins are available for:
    ///     - JetBrains ReSharper        https://nuke.build/resharper
    ///     - JetBrains Rider            https://nuke.build/rider
    ///     - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///     - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>
    public static int Main()
    {
        return Execute<Pipeline>(x => x.Default);
    }

    [OptionalGitRepository]
    public GitRepository? GitRepository { get; }

    [Parameter]
    public string? GitHubToken { get; }

    private Target Default => _ => _
                                  .DependsOn(Restore)
                                  .DependsOn(Build)
                                  .DependsOn(Test)
                                  .DependsOn(Pack);

    [Solution(GenerateProjects = true)]
    private Solution Solution { get; } = null!;

    public Target Build => _ => _.Inherit<ICanBuildWithDotNetCore>(x => x.CoreBuild);
    public Target Lint => _ => _.Inherit<ICanLint>(x => x.Lint);

    public Target Pack => _ => _
                              .Inherit<ICanPackWithDotNetCore>(x => x.CorePack)
                              .DependsOn(Clean);


    public Target Clean => _ => _.Inherit<ICanClean>(x => x.Clean);
    public Target Restore => _ => _.Inherit<ICanRestoreWithDotNetCore>(x => x.CoreRestore);
    Nuke.Common.ProjectModel.Solution IHaveSolution.Solution => Solution;

    [GitVersion(NoFetch = true)]
    public GitVersion GitVersion { get; } = null!;

    public Target Test => _ => _.Inherit<ICanTestWithDotNetCore>(x => x.CoreTest);

    [Parameter("Configuration to build")]
    public Configuration Configuration { get; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;
}

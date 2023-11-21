using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.GithubActions;

#pragma warning disable CA1050

[GitHubActionsSteps(
    "ci-ignore",
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = new[] { RocketSurgeonGitHubActionsTrigger.Push },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master", "main", "next" },
    OnPullRequestBranches = new[] { "master", "main", "next" },
    Enhancements = new[] { nameof(CiIgnoreMiddleware) }
)]
[GitHubActionsSteps(
    "ci",
    GitHubActionsImage.MacOsLatest,
    GitHubActionsImage.WindowsLatest,
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = new[]
    {
        RocketSurgeonGitHubActionsTrigger.WorkflowCall,
        RocketSurgeonGitHubActionsTrigger.WorkflowDispatch
    },
    OnPushTags = new[] { "v*" },
    OnPushBranches = new[] { "master", "main", "next" },
    OnPullRequestBranches = new[] { "master", "main", "next" },
    InvokedTargets = new[] { nameof(Default) },
    NonEntryTargets = new[]
    {
        nameof(ICIEnvironment.CIEnvironment),
        nameof(ITriggerCodeCoverageReports.TriggerCodeCoverageReports),
        nameof(ITriggerCodeCoverageReports.GenerateCodeCoverageReportCobertura),
        nameof(IGenerateCodeCoverageBadges.GenerateCodeCoverageBadges),
        nameof(IGenerateCodeCoverageReport.GenerateCodeCoverageReport),
        nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary),
        nameof(Default)
    },
    ExcludedTargets = new[] { nameof(ICanClean.Clean), nameof(ICanRestoreWithDotNetCore.DotnetToolRestore) },
    Enhancements = new[] { nameof(CiMiddleware) }
)]
[GitHubActionsLint(
    "lint",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    OnPullRequestTargetBranches = new[] { "master", "main", "next" },
    Enhancements = new[] { nameof(LintStagedMiddleware) }
)]
[GitHubActionsSteps(
    "inputs",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = new[] { RocketSurgeonGitHubActionsTrigger.WorkflowCall },
    InvokedTargets = new[] { nameof(WithOutputs) }
)]
[GitHubActionsVariable("THIS_IS_A_VARIABLE", Alias = "ThisIsAOtherVariable")]
[GitHubActionsVariable("THIS_IS_ANOTHER_VARIABLE")]
[GitHubActionsInput("THIS_IS_A_INPUT" /*, Alias = "ThisIsAInput"*/)]
[GitHubActionsInput("THIS_IS_ANOTHER_INPUT" /*, Alias = "ThisIsAInput"*/)]
[GitHubActionsEnvironmentVariable("THIS_IS_A_ENV" /*, Alias = "ThisIsAEnv"*/, Default = "'test'")]
[GitHubActionsSecret("THIS_IS_A_SECRET" /*, Alias = "ThisIsASecret"*/)]
[GitHubActionsSecret("RSG_BOT_TOKEN")]
[PrintBuildVersion]
[PrintCIEnvironment]
[UploadLogs]
[TitleEvents]
[ContinuousIntegrationConventions]
public partial class Pipeline
{
    public static RocketSurgeonGitHubActionsConfiguration CiIgnoreMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        ( (RocketSurgeonsGithubActionsJob)configuration.Jobs[0] ).Steps = new List<GitHubActionsStep>
        {
            new RunStep("N/A")
            {
                Run = "echo \"No build required\""
            }
        };

        return configuration.IncludeRepositoryConfigurationFiles();
    }

    private Target WithOutputs => _ => _.ProducesGithubActionsOutput("iSetAThing", "Some output value")
                                        .Requires(() => ThisIsAInput)
                                        .Executes(() => GitHubActions.Instance?.SetOutput("iSetAThing", "myValue"));

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Parameter] public string ThisIsAInput { get; set; }
    [Parameter] public string ThisIsAnotherInput { get; set; }
    [Parameter] public string ThisIsADifferentOutput { get; set; }
    [Parameter] public string ThisIsAOtherVariable { get; set; }
    [Parameter] public string ThisIsAnotherVariable { get; set; }
    [Parameter(Name = "THIS_IS_A_VARIABLE")] public string ThisIsAVariable { get; set; }
    [Parameter] public string ThisIsAEnv { get; set; }
    [Parameter] public string ThisIsASecret { get; set; }

    public static RocketSurgeonGitHubActionsConfiguration CiMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration
           .ExcludeRepositoryConfigurationFiles()
           .AddNugetPublish()
           .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
           .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase))
           .UseDotNetSdks("6.0", "8.0")
           .AddNuGetCache()
           // .ConfigureForGitVersion()
           .ConfigureStep<CheckoutStep>(step => step.FetchDepth = 0)
           .PublishLogs<Pipeline>();

        return configuration;
    }

    public static RocketSurgeonGitHubActionsConfiguration LintStagedMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration
           .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
           .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase))
           .UseDotNetSdks("6.0", "8.0");

        return configuration;
    }
}

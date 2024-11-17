using System.Diagnostics;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.GithubActions;

#pragma warning disable CA1050

[GitHubActionsSteps(
    "ci-ignore",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = [RocketSurgeonGitHubActionsTrigger.Push,],
    OnPushTags = ["v*",],
    OnPushBranches = ["master", "main", "next",],
    OnPullRequestBranches = ["master", "main", "next",],
    Enhancements = [nameof(CiIgnoreMiddleware),]
)]
[GitHubActionsSteps(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On =
    [
        RocketSurgeonGitHubActionsTrigger.WorkflowCall,
        RocketSurgeonGitHubActionsTrigger.WorkflowDispatch,
    ],
    OnPushTags = ["v*",],
    OnPushBranches = ["master", "main", "next",],
    OnPullRequestBranches = ["master", "main", "next",],
    InvokedTargets = [nameof(Default),],
    NonEntryTargets = [nameof(Default),],
    Enhancements = [nameof(CiMiddleware),]
)]
[GitHubActionsLint(
    "lint",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    OnPullRequestTargetBranches = ["master", "main", "next",],
    Enhancements = [nameof(LintStagedMiddleware),]
)]
[GitHubActionsSteps(
    "inputs",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    On = [RocketSurgeonGitHubActionsTrigger.WorkflowCall,],
    InvokedTargets = [nameof(WithOutputs),]
)]
[GitHubActionsVariable("THIS_IS_A_VARIABLE", Alias = "ThisIsAOtherVariable")]
[GitHubActionsVariable("THIS_IS_ANOTHER_VARIABLE")]
[GitHubActionsInput("THIS_IS_A_INPUT" /*, Alias = "ThisIsAInput"*/)]
[GitHubActionsInput("THIS_IS_ANOTHER_INPUT" /*, Alias = "ThisIsAInput"*/)]
[GitHubActionsEnvironmentVariable("THIS_IS_A_ENV" /*, Alias = "ThisIsAEnv"*/, Default = "'test'")]
[GitHubActionsSecret("THIS_IS_A_SECRET" /*, Alias = "ThisIsASecret"*/)]
// used for testing
//[OnePasswordSecret("MY_OTHER_ONEPASSWORD_TEXT", "ONEPASSWORDITEM", "text", UseServiceAccount = true)]
//[OnePasswordSecret("MY_ONEPASSWORD_TEXT", "op://somevault/someitem/text", UseConnectServer = true)]
//[OnePasswordServiceAccountSecret("MY_SA_OTHER_ONEPASSWORD_TEXT", "ONEPASSWORDITEM", "text")]
//[OnePasswordServiceAccountSecret("MY_SA_ONEPASSWORD_TEXT", "op://somevault/someitem/text")]
//[OnePasswordConnectServerSecret("MY_CONNECT_OTHER_ONEPASSWORD_TEXT", "ONEPASSWORDITEM", "text")]
//[OnePasswordConnectServerSecret("MY_CONNECT_ONEPASSWORD_TEXT", "op://somevault/someitem/text")]
[PrintBuildVersion]
[PrintCIEnvironment]
[UploadLogs]
[TitleEvents]
[ContinuousIntegrationConventions]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal partial class Pipeline
{
    public static RocketSurgeonGitHubActionsConfiguration CiIgnoreMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        ( (RocketSurgeonsGithubActionsJob)configuration.Jobs[0] ).Steps =
        [
            new RunStep("N/A")
            {
                Run = "echo \"No build required\"",
            },
        ];

        return configuration.IncludeRepositoryConfigurationFiles();
    }

    private Target WithOutputs => _ => _
                                      .ProducesGithubActionsOutput("iSetAThing", "Some output value")
                                      .Requires(() => ThisIsAInput)
                                      .Executes(() => GitHubActions.Instance?.SetOutput("iSetAThing", "myValue"));
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Parameter]
    public string ThisIsAInput { get; set; }

    [Parameter]
    public string ThisIsAnotherInput { get; set; }

    [Parameter]
    public string ThisIsADifferentOutput { get; set; }

    [Parameter]
    public string ThisIsAOtherVariable { get; set; }

    [Parameter]
    public string ThisIsAnotherVariable { get; set; }

    [Parameter(Name = "THIS_IS_A_VARIABLE")]
    public string ThisIsAVariable { get; set; }

    [Parameter]
    public string ThisIsAEnv { get; set; }

    [Parameter]
    public string ThisIsASecret { get; set; }

    [Parameter(Name = "MY_ONEPASSWORD_TEXT")]
    public string MyOnepasswordText { get; set; }

    [Parameter]
    public string MyOtherOnepasswordText { get; set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    public static RocketSurgeonGitHubActionsConfiguration CiMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        _ = configuration
           .ExcludeRepositoryConfigurationFiles()
           .AddNugetPublish()
           .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
           .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase))
           .UseDotNetSdks("8.0", "9.0")
            // .ConfigureForGitVersion()
           .ConfigureStep<CheckoutStep>(step => step.FetchDepth = 0)
           .PublishLogs<Pipeline>();

        return configuration;
    }

    public static RocketSurgeonGitHubActionsConfiguration LintStagedMiddleware(RocketSurgeonGitHubActionsConfiguration configuration)
    {
        _ = configuration
           .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
           .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase))
           .UseDotNetSdks("8.0", "9.0");

        return configuration;
    }
}

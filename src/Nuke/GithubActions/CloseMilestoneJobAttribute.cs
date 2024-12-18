using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Adds draft release support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class CloseMilestoneJobAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute() : base("close-milestone", GitHubActionsImage.UbuntuLatest) { }

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute(string image, params string[] images) : base("close-milestone", image, images) { }

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute(GitHubActionsImage image) : base("close-milestone", image) { }

    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Close Milestone"
        };
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowCall,
                Secrets = [new GitHubActionsSecret("RSG_BOT_TOKEN", Required: true)],
            }
        );
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.Release,
                Types = ["released"],
            }
        );
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("close_milestone")
            {
                RunsOn = ( !IsGithubHosted ) ? Images : [],
                Matrix = IsGithubHosted ? Images : [],
                Steps =
                [
                    new CheckoutStep("Checkout")
                    {
                        FetchDepth = 0,
                    },
                    new RunStep("Fetch all history for all tags and branches")
                    {
                        Run = "git fetch --prune",
                    },
                    new SetupDotNetStep("Install DotNet"),
                    new RunStep("dotnet tool restore")
                    {
                        Run = "dotnet tool restore",
                    },
                    new UsingStep("Install GitVersion")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "gittools/actions/gitversion/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitVersion.Tool").Version
                        },
                    },
                    new UsingStep("Install GitReleaseManager")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "gittools/actions/gitreleasemanager/setup@v3.1.1",
                        With =
                        {
                            ["versionSpec"] = DotNetTool.GetToolDefinition("GitReleaseManager.Tool").Version
                        },
                    },
                    new UsingStep("Use GitVersion")
                    {
                        Id = "gitversion",
                        Uses = "gittools/actions/gitversion/execute@v3.1.1",
                        With =
                        {
                            ["useConfigFile"] = "true",
                        }
                    },
                    new UsingStep("Create Milestone")
                    {
                        If = "${{ github.event.action == 'opened' }}",
                        Uses = "WyriHaximus/github-action-create-milestone@v1",
                        With =
                        {
                            ["title"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}"
                        },
                        Environment =
                        {
                            ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                        ContinueOnError = true,
                    },
                    new UsingStep("sync milestones")
                    {
                        Uses = "RocketSurgeonsGuild/actions/sync-milestone@v0.3.15",
                        With =
                        {
                            ["default-label"] = ":sparkles: mysterious",
                            ["github-token"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                    },
                    new RunStep("Get Repo and Owner")
                    {
                        Shell = "pwsh",
                        Id = "repository",
                        If = "${{ !github.event.release.prerelease && steps.gitversion.outputs.preReleaseTag == '' }}",
                        Run = """
                            $parts = $ENV:GITHUB_REPOSITORY.Split('/')
                            echo "::set-output name=owner::$($parts[0])"
                            echo "::set-output name=repository::$($parts[1])"
                            """
                    },
                    new RunStep("Close Milestone")
                    {
                        Shell = "pwsh",
                        If = "${{ !github.event.release.prerelease && steps.gitversion.outputs.preReleaseTag == '' }}",
                        Run = """
                            dotnet gitreleasemanager close `
                                -o "${{ steps.repository.outputs.owner }}" `
                                -r "${{ steps.repository.outputs.repository }}" `
                                --token "${{ secrets.GITHUB_TOKEN }}" `
                                -m "v${{ steps.gitversion.outputs.majorMinorPatch }}"
                            """
                    },
                ]
            }
        );

        return build;
    }
}

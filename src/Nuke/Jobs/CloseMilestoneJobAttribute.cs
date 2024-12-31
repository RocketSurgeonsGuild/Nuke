using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke.Jobs;

/// <summary>
/// Adds draft release support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class CloseMilestoneJobAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute() : base("close-milestone", GitHubActionsImage.UbuntuLatest)
    {
        AutoGenerate = false;
    }

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute(string image, params string[] images) : base("close-milestone", image, images)
    {
        AutoGenerate = false;
    }

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public CloseMilestoneJobAttribute(GitHubActionsImage image) : base("close-milestone", image)
    {
        AutoGenerate = false;
    }

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
        var job = this.CreateJob(
            "close_milestone",
            true,
            [
                ..WorkflowHelpers.RunGitVersion(),
                ..WorkflowHelpers.CreateMilestone(),
                ..WorkflowHelpers.SyncMilestones(),
                ..WorkflowHelpers.InstallGitReleaseManager(),
                new RunStep("Get Repo and Owner")
                {
                    Shell = "pwsh",
                    Id = "repository",
                    If = "${{ !github.event.release.prerelease && steps.gitversion.outputs.preReleaseTag == '' }}",
                    Run = """
                        $parts = $ENV:GITHUB_REPOSITORY.Split('/')
                        echo "::set-output name=owner::$($parts[0])"
                        echo "::set-output name=repository::$($parts[1])"
                        """,
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
                        """,
                }
            ]
        );
        build.Jobs.Add(job);

        return build;
    }
}

using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// Adds update milestone support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class UpdateMilestoneJobAttribute() : GitHubActionsStepsAttribute("update-milestone", GitHubActionsImage.UbuntuLatest)
{
    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Update Milestone"
        };
        build.DetailedTriggers.Add(new RocketSurgeonGitHubActionsWorkflowTrigger());
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.PullRequestTarget,
                Types = ["closed", "opened", "reopened", "synchronize"],
            }
        );
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("update_milestone")
            {
                RunsOn = ( !IsGithubHosted ) ? Images : [],
                Matrix = IsGithubHosted ? Images : [],
                Steps =
                [
                    new CheckoutStep("Checkout")
                    {
                        Ref = "${{ github.sha }}",
                        FetchDepth = 0,
                    },
                    new RunStep("Fetch all history for all tags and branches")
                    {
                        Run = "git fetch --prune",
                    },
                    new SetupDotNetStep("Install DotNet"),
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
                        If = "${{ github.event.action == 'opened' }}",
                        Id = "gitversion",
                        Uses = "gittools/actions/gitversion/execute@v3.1.1",
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
                            ["default-label"] = ":sparkles: mysterious"
                        },
                        Environment =
                        {
                            ["github-token"] = "${{ secrets.GITHUB_TOKEN }}"
                        },
                    },
                ]
            }
        );

        return build;
    }
}

/// <summary>
/// Adds draft release support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class DraftReleaseJobAttribute() : GitHubActionsStepsAttribute("draft-release", GitHubActionsImage.UbuntuLatest)
{
    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Draft Release and Create Milestone"
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
                Kind = RocketSurgeonGitHubActionsTrigger.Push,
                Branches = ["master"],
                ExcludePaths = ["**/*.md"],
            }
        );
        build.DetailedTriggers.Add(new GitHubActionsScheduledTrigger { Cron = "0 0 * * 4" });
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("draft_release")
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
                    },
                    new UsingStep("sync milestones")
                    {
                        Uses = "RocketSurgeonsGuild/actions/sync-milestone@v0.3.15",
                        With =
                        {
                            ["default-label"] = ":sparkles: mysterious",
                            ["github-token"] = "${{ secrets.GITHUB_TOKEN }"
                        }
                    },

                    new UsingStep("Create Release")
                    {
                        Uses = "ncipollo/release-action@v1",
                        With =
                        {
                            ["allowUpdates"] = "true", ["generateReleaseNotes"] = "true",
                            ["draft"] = "true",
                            ["omitNameDuringUpdate"] = "true",
                            ["name"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                            ["tag"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
                            ["token"] = "${{ secrets.RSG_BOT_TOKEN }}",
                            ["commit"] = "${{ github.base_ref }}"
                        },
                    },
                ]
            }
        );

        return build;
    }
}

/// <summary>
/// Adds close milestone support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class PublishNugetPackagesJobAttribute : GitHubActionsStepsAttribute
{
    private readonly string _secretKey;

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey) : base("publish-nuget", GitHubActionsImage.UbuntuLatest) => _secretKey = secretKey;

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey, string image, params string[] images) : base("publish-nuget", image, images) => _secretKey = secretKey;

    /// <summary>
    /// Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey, GitHubActionsImage image) : base("publish-nuget", image) => _secretKey = secretKey;

    private string DebuggerDisplay => ToString();

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Publish Nuget Packages"
        };
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowCall,
                Secrets = [new GitHubActionsSecret(_secretKey, Required: true)],
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
            new RocketSurgeonsGithubActionsJob("publish_nuget")
            {
                RunsOn = ( !IsGithubHosted ) ? Images : [],
                Matrix = IsGithubHosted ? Images : [],
                Steps =
                [
                    new DownloadArtifactStep("nuget"),
                    new RunStep("nuget.org")
                    {
                        If = "startsWith(github.ref, 'refs/tags/')",
                        Shell = "pwsh",
                        Environment =
                        {
                            ["ApiKey"] = $"${{{{ secrets.{_secretKey} }}}}"
                        },
                        Run = @"
                            dotnet nuget push **/*.nupkg  --skip-duplicate -s nuget.org --api-key $ENV:ApiKey
                            dotnet nuget push **/*.snupkg --skip-duplicate -s nuget.org --api-key $ENV:ApiKey
                        "
                    },
                ]
            }
        );

        return build;
    }
}

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
                            ["default-label"] = ":sparkles: mysterious"
                        },
                        Environment =
                        {
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

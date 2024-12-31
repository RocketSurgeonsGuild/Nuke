using Rocket.Surgery.Nuke.GithubActions;

namespace Rocket.Surgery.Nuke.Jobs;

/// <summary>
///     Common helpers for creating github workflows
/// </summary>
public static class WorkflowHelpers
{
    /// <summary>
    ///     Create a job with the correct runson matrix
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="name"></param>
    /// <param name="fetchHistory"></param>
    /// <param name="steps"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob CreateJob(
        this GitHubActionsStepsAttribute attribute,
        string name,
        bool fetchHistory = false,
        params IEnumerable<BaseGitHubActionsStep> steps
    ) => new(name)
    {
        RunsOn = !attribute.IsGithubHosted ? attribute.Images : [],
        Matrix = attribute.IsGithubHosted ? attribute.Images : [],
        Steps =
        [
            new CheckoutStep("Checkout")
            {
                FetchDepth = fetchHistory ? 0 : null,
            },
            ..fetchHistory
                ?
                [
                    new RunStep("Fetch all history for all tags and branches")
                    {
                        Run = "git fetch --prune",
                    },
                ]
                : Array.Empty<BaseGitHubActionsStep>(),
            new SetupDotNetStep("Install DotNet"),
            new RunStep("dotnet tool restore")
            {
                Run = "dotnet tool restore",
            },
            ..steps,
        ],
    };

    /// <summary>
    ///     Install the git release manager
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static IEnumerable<BaseGitHubActionsStep> InstallGitReleaseManager(GithubActionCondition? condition = null)
    {
        yield return new UsingStep("Install GitReleaseManager")
        {
            If = condition,
            Uses = "gittools/actions/gitreleasemanager/setup@v3.1.1",
            With =
            {
                ["versionSpec"] = DotNetTool.GetToolDefinition("GitReleaseManager.Tool").Version,
            },
        };
    }

    /// <summary>
    ///     Create a milestone
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static IEnumerable<BaseGitHubActionsStep> CreateMilestone(GithubActionCondition? condition = null)
    {
        yield return new UsingStep("Create Milestone")
        {
            If = condition,
            Uses = "WyriHaximus/github-action-create-milestone@v1",
            With =
            {
                ["title"] = "v${{ steps.gitversion.outputs.majorMinorPatch }}",
            },
            Environment =
            {
                ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
            },
        };
    }

    /// <summary>
    ///     Sync the milestones
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static IEnumerable<BaseGitHubActionsStep> SyncMilestones(GithubActionCondition? condition = null)
    {
        yield return new UsingStep("sync milestones")
        {
            If = condition,
            Uses = "RocketSurgeonsGuild/actions/sync-milestone@v0.3.15",
            With =
            {
                ["default-label"] = ":sparkles: mysterious",
                ["github-token"] = "${{ secrets.GITHUB_TOKEN }}",
            },
        };
    }

    /// <summary>
    ///     Run gitversion
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static IEnumerable<BaseGitHubActionsStep> RunGitVersion(GithubActionCondition? condition = null)
    {
        yield return new RunStep("Use GitVersion")
        {
            If = condition,
            Id = "gitversion",
            Outputs =
            [
                new("assemblySemFileVer"),
                new("assemblySemVer"),
                new("branchName"),
                new("buildMetaData"),
                new("commitDate"),
                new("commitsSinceVersionSource"),
                new("escapedBranchName"),
                new("fullBuildMetaData"),
                new("fullSemVer"),
                new("informationalVersion"),
                new("major"),
                new("majorMinorPatch"),
                new("minor"),
                new("patch"),
                new("preReleaseLabel"),
                new("preReleaseLabelWithDash"),
                new("preReleaseNumber"),
                new("preReleaseTag"),
                new("preReleaseTagWithDash"),
                new("semVer"),
                new("sha"),
                new("shortSha"),
                new("uncommittedChanges"),
                new("versionSourceSha"),
                new("weightedPreReleaseNumber"),
            ],
            Shell = "pwsh",
            Run = """
                                $data = dotnet gitversion | ConvertFrom-Json
                                foreach ($item in $data.PSObject.Properties) {
                                    $key = $item.Name
                                    $value = $item.Value
                                    echo "$($key.Substring(0,1).ToLower() + $key.Substring(1))=$value" >> $env:GITHUB_OUTPUT
                                }
                """,
        };
    }
}

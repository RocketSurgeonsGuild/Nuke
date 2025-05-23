using System.Collections.Immutable;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.GithubActions;

#pragma warning disable RS0026, RS0027

namespace Rocket.Surgery.Nuke.Jobs;

/// <summary>
///     Adds close milestone support to the build
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public sealed class PublishNugetPackagesJobAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey, string triggeringWorkflow, string[]? includeBranches = null, string? nugetOrgCondition = null) : base(
        "publish-nuget",
        GitHubActionsImage.UbuntuLatest
    )
    {
        _secretKey = secretKey;
        _triggeringWorkflow = triggeringWorkflow;
        _nugetOrgCondition = nugetOrgCondition ?? _nugetOrgCondition;
        _includeBranches = [.. includeBranches ?? []];
        AutoGenerate = false;
    }

    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey, string triggeringWorkflow, string[] includeBranches, string? nugetOrgCondition, string image, params string[] images) : base(
        "publish-nuget",
        image,
        images
    )
    {
        _secretKey = secretKey;
        _triggeringWorkflow = triggeringWorkflow;
        _nugetOrgCondition = nugetOrgCondition ?? _nugetOrgCondition;
        _includeBranches = [.. includeBranches];
        AutoGenerate = false;
    }

    /// <summary>
    ///     Adds draft release support to the build
    /// </summary>
    public PublishNugetPackagesJobAttribute(string secretKey, string triggeringWorkflow, GitHubActionsImage image, string[]? includeBranches = null, string? nugetOrgCondition = null) : base(
        "publish-nuget",
        image
    )
    {
        _secretKey = secretKey;
        _triggeringWorkflow = triggeringWorkflow;
        _nugetOrgCondition = nugetOrgCondition ?? _nugetOrgCondition;
        _includeBranches = [.. includeBranches ?? []];
        AutoGenerate = false;
    }

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var build = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = "Publish Nuget Packages",
        };
        build.DetailedTriggers.Add(
            new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowRun,
                Types = ["completed"],
                Workflows = [_triggeringWorkflow],
                Branches = [.. _includeBranches],
            }
        );
        build.Jobs.Add(
            new RocketSurgeonsGithubActionsJob("publish_nuget")
            {
                RunsOn = !IsGithubHosted ? Images : [],
                Matrix = IsGithubHosted ? Images : [],
                If = "${{ github.event.workflow_run.conclusion == 'success' }}",
                Steps =
                [
                    new RunStep("Dump GitHub context")
                    {
                        Shell = "pwsh",
                        Environment = { ["GITHUB_CONTEXT"] = "${{ toJson(github) }}" },
                        Run = "echo \"$GITHUB_CONTEXT\"",
                    },
                    new DownloadArtifactStep("NuGet")
                    {
                        GithubToken = "${{ secrets.GITHUB_TOKEN }}",
                        Name = "nuget",
                        RunId = "${{ github.event.workflow_run.id }}",
                    },
                    new RunStep("nuget.org")
                    {
                        If = _nugetOrgCondition,
                        Shell = "pwsh",
                        Environment =
                        {
                            ["ApiKey"] = $"${{{{ secrets.{_secretKey} }}}}",
                        },
                        Run = @"
                            dotnet nuget push **/*.nupkg  --skip-duplicate -s nuget.org --api-key $ENV:ApiKey
                            dotnet nuget push **/*.snupkg --skip-duplicate -s nuget.org --api-key $ENV:ApiKey
                        ",
                    },
                ],
            }
        );

        return build;
    }

    private readonly ImmutableArray<string> _includeBranches;

    private readonly GithubActionCondition _nugetOrgCondition =
        "startsWith(github.event.workflow_run.head_branch, 'v') && contains(github.event.workflow_run.head_branch, '.') && !contains(github.event.workflow_run.head_branch, '/')";

    private readonly string _secretKey;
    private readonly string _triggeringWorkflow;
}

using System.Diagnostics;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

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

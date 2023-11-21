using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
#pragma warning disable CA1019

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
/// An attribute to help adding the lint workflow
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class GitHubActionsLintAttribute : GitHubActionsStepsAttribute
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="image"></param>
    /// <param name="images"></param>
    public GitHubActionsLintAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images
    ) : base(name, image, images)
    {
        InvokedTargets = new[] { nameof(ICanLintStagedFiles.LintStaged) };
    }

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="image"></param>
    /// <param name="images"></param>
    public GitHubActionsLintAttribute(
        string name,
        string image,
        params string[] images
    ) : base(name, image, images)
    {
    }

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var config = base.GetConfiguration(relevantTargets);
        if (config is not RocketSurgeonGitHubActionsConfiguration configuration)
        {
            return config;
        }

        var buildJob =
            configuration
               .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
               .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase));

        configuration.Permissions.Contents = GitHubActionsPermission.Write;

        buildJob
           .ConfigureStep<CheckoutStep>(
                step =>
                {
                    step.FetchDepth = 0;
                    step.Repository = "${{ github.event.pull_request.head.repo.full_name }}";
                    step.Ref = "${{ github.event.pull_request.head.ref }}";
                }
            )
           .AddStep(
                new UsingStep("Add & Commit")
                {
                    Uses = "stefanzweifel/git-auto-commit-action@v5",
                    With = { ["commit_message"] = "Automatically linting code", }
                }
            );

        buildJob.Name = "lint";

        return configuration;
    }
}

using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;

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

    /// <summary>
    /// The PAT token that is used to access the repository
    /// </summary>
    /// <remarks>
    /// Should be in the format of the name of the secret eg RSG_BOT_TOKEN
    /// </remarks>
    public string TokenSecret { get; set; } = "RSG_BOT_TOKEN";

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
                    step.Token = "${{ secrets.RSG_BOT_TOKEN }}";
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

        configuration.Jobs
                     .OfType<RocketSurgeonsGithubActionsJob>()
                     .SelectMany(z => z.Steps)
                     .OfType<BaseGitHubActionsStep>()
                     .ForEach(z => z.If = "github.event.pull_request.user.login != 'renovate[bot]' && github.event.pull_request.user.login != 'dependabot[bot]'");

        buildJob.Name = "lint";

        return configuration;
    }
}

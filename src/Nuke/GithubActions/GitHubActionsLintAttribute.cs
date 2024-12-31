using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;

#pragma warning disable CA1019

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     An attribute to help adding the lint workflow
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
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
    ) : base(name, image, images) => InvokedTargets = [nameof(ICanLint.Lint)];

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
    ) : base(name, image, images) { }

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var config = base.GetConfiguration(relevantTargets);
        if (config is not RocketSurgeonGitHubActionsConfiguration configuration) return config;

        var buildJob =
            configuration
               .Jobs.OfType<RocketSurgeonsGithubActionsJob>()
               .First(z => z.Name.Equals("Build", StringComparison.OrdinalIgnoreCase));

        configuration.Permissions.Contents = GitHubActionsPermission.Write;

        const string commitMessage = "Automatically linting code";

        var secretPath = Build
                        .GetType()
                        .GetCustomAttributes()
                        .OfType<TriggerValueAttribute>()
                        .Where(z => z.Name == TokenSecret)
                        .Select(z => z.ToTriggerValue())
                        .Select(value => string.IsNullOrWhiteSpace(value.Prefix) ? value.Name : $"{value.Prefix}.{value.Name}")
                        .FirstOrDefault()
         ?? ( TokenSecret.Contains('.') ? $"{TokenSecret}" : $"secrets.{TokenSecret}" );

        configuration.Concurrency = new()
        {
            CancelInProgress = true,
            Group = "lint-${{ github.event.pull_request.number }}",
        };

        buildJob
           .ConfigureStep<CheckoutStep>(
                step =>
                {
                    step.Token ??= $$$"""${{ {{{secretPath}}} }}""";
                    step.FetchDepth = 0;
                    step.Repository = "${{ github.event.pull_request.head.repo.full_name }}";
                    step.Ref = "${{ github.event.pull_request.head.ref }}";
                }
            )
           .InsertAfterCheckOut(
                new RunStep("npm ci --ignore-scripts") { Run = "npm ci --ignore-scripts" }
            )
           .InsertAfterCheckOut(
                new RunStep("Get Head Commit Message") { Id = "commit-message", Run = "echo \"message=$(git show -s --format=%s)\" >> \"$GITHUB_OUTPUT\"" }
            )
           .AddStep(
                new UsingStep("Add & Commit")
                {
                    If = $$$""" "contains('${{ steps.commit-message.outputs.message }}', '{{{commitMessage}}}')" """.Trim(),
                    Uses = "planetscale/ghcommit-action@v0.1.36",
                    With =
                    {
                        ["commit_message"] = commitMessage,
                        ["repo"] = "${{ github.repository }}",
                        ["branch"] = "${{ github.event.pull_request.head.ref }}",
                    },
                    Environment = { ["GITHUB_TOKEN"] = $$$"""${{ {{{secretPath}}} }}""" },
                }
            );

        foreach (var workflowTrigger in configuration
                                       .DetailedTriggers
                                       .OfType<RocketSurgeonGitHubActionsWorkflowTrigger>()
                )
        {
            if (workflowTrigger.Secrets.Any(z => z.Name == TokenSecret || z.Alias == TokenSecret)) continue;

            workflowTrigger.Secrets.Add(new(TokenSecret, "The token used to commit back when linting", true));
        }

        buildJob.Name = "lint";

        NormalizeActionVersions(configuration);
        return configuration;
    }

    /// <summary>
    ///     The PAT token that is used to access the repository
    /// </summary>
    /// <remarks>
    ///     Should be in the format of the name of the secret eg RSG_BOT_TOKEN
    /// </remarks>
    public string TokenSecret { get; set; } = "RSG_BOT_TOKEN";
}

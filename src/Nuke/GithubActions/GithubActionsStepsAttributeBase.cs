using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using YamlDotNet.RepresentationModel;

#pragma warning disable CA1851
// ReSharper disable PossibleMultipleEnumeration
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Base attribute for a github actions workflow
/// </summary>
[PublicAPI]
public abstract class GithubActionsStepsAttributeBase : ChainedConfigurationAttributeBase
{
    /// <summary>
    ///     The default constructor given the file name
    /// </summary>
    /// <param name="name"></param>
    protected GithubActionsStepsAttributeBase(string name)
    {
        Name = name;
        ExcludedTargets =
        [
            ..ExcludedTargets,
            nameof(ICanClean.Clean),
            nameof(ICanRestoreWithDotNetCore.DotnetToolRestore),
            nameof(ICanRestoreWithDotNetCore.DotnetWorkloadRestore),
        ];
    }

    public new string[] NonEntryTargets
    {
        get => base.NonEntryTargets;
        set => base.NonEntryTargets =
        [
            ..value,
            nameof(ICIEnvironment.CIEnvironment),
            nameof(ITriggerCodeCoverageReports.TriggerCodeCoverageReports),
            nameof(ITriggerCodeCoverageReports.GenerateCodeCoverageReportCobertura),
            nameof(IGenerateCodeCoverageBadges.GenerateCodeCoverageBadges),
            nameof(IGenerateCodeCoverageReport.GenerateCodeCoverageReport),
            nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary),
        ];
    }

    public new string[] ExcludedTargets
    {
        get => base.ExcludedTargets;
        set => base.ExcludedTargets =
        [
            ..value,
            nameof(ICanClean.Clean),
            nameof(ICanRestoreWithDotNetCore.DotnetToolRestore),
            nameof(ICanRestoreWithDotNetCore.DotnetWorkloadRestore),
        ];
    }

    /// <inheritdoc />
    public override Type HostType { get; } = typeof(GitHubActions);

    /// <inheritdoc />
    public override AbsolutePath ConfigurationFile => NukeBuild.RootDirectory / ".github" / "workflows" / $"{Name}.yml";

    /// <summary>
    ///     The triggers
    /// </summary>
    public RocketSurgeonGitHubActionsTrigger[] On { get; set; } = [];

    /// <summary>
    ///     The branches to run for push
    /// </summary>
    public string[] OnPushBranches { get; set; } = [];

    /// <summary>
    ///     The tags to run for push
    /// </summary>
    public string[] OnPushTags { get; set; } = [];

    /// <summary>
    ///     The paths to include for pushes
    /// </summary>
    public string[] OnPushIncludePaths { get; set; } = [];

    /// <summary>
    ///     The paths to exclude for pushes
    /// </summary>
    public string[] OnPushExcludePaths { get; set; } = [];

    /// <summary>
    ///     The branches for pull requests
    /// </summary>
    public string[] OnPullRequestBranches { get; set; } = [];

    /// <summary>
    ///     The tags for pull requests
    /// </summary>
    public string[] OnPullRequestTags { get; set; } = [];

    /// <summary>
    ///     The paths to include for pull requests
    /// </summary>
    public string[] OnPullRequestIncludePaths { get; set; } = [];

    /// <summary>
    ///     The paths to exclude for pull requests
    /// </summary>
    public string[] OnPullRequestExcludePaths { get; set; } = [];

    /// <summary>
    ///     The branches for pull requests
    /// </summary>
    public string[] OnPullRequestTargetBranches { get; set; } = [];

    /// <summary>
    ///     The tags for pull requests
    /// </summary>
    public string[] OnPullRequestTargetTags { get; set; } = [];

    /// <summary>
    ///     The paths to include for pull requests
    /// </summary>
    public string[] OnPullRequestTargetIncludePaths { get; set; } = [];

    /// <summary>
    ///     The paths to exclude for pull requests
    /// </summary>
    public string[] OnPullRequestTargetExcludePaths { get; set; } = [];

    /// <summary>
    ///     The schedule to run on
    /// </summary>
    public string? OnCronSchedule { get; set; }

    /// <summary>
    ///     A list of static methods that can be used for additional configurations
    /// </summary>
    public string[] Enhancements { get; set; } = [];

    /// <summary>
    ///     The name of the file
    /// </summary>
    protected string Name { get; }

    /// <summary>
    ///     Applies the given enhancements to the build
    /// </summary>
    /// <param name="config"></param>
    protected void ApplyEnhancements(RocketSurgeonGitHubActionsConfiguration config)
    {
        if (Enhancements.Length > 0)
        {
            foreach (var method in Enhancements.Join(Build.GetType().GetMethods(), z => z, z => z.Name, (_, e) => e))
            {
                config = method.IsStatic
                    ? method.Invoke(null, new object[] { config, }) as RocketSurgeonGitHubActionsConfiguration ?? config
                    : method.Invoke(Build, new object[] { config, }) as RocketSurgeonGitHubActionsConfiguration
                 ?? config;
            }
        }

        // This will normalize the version numbers against the existing file.
        if (!File.Exists(ConfigurationFile)) return;

        using var readStream = File.OpenRead(ConfigurationFile);
        using var reader = new StreamReader(readStream);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);
        var key = new YamlScalarNode("uses");
        var nodeList = yamlStream
                      .Documents
                      .SelectMany(z => z.AllNodes)
                      .OfType<YamlMappingNode>()
                      .Where(
                           z => z.Children.ContainsKey(key)
                            && z.Children[key] is YamlScalarNode sn
                            && sn.Value?.Contains('@', StringComparison.OrdinalIgnoreCase) == true
                       )
                      .Select(
                           // ReSharper disable once NullableWarningSuppressionIsUsed
                           z => ( name: ( (YamlScalarNode)z.Children[key] ).Value!.Split("@")[0],
                                  value: ( (YamlScalarNode)z.Children[key] ).Value )
                       )
                      .Distinct(z => z.name)
                      .ToDictionary(
                           z => z.name,
                           z => z.value
                       );

        string? GetValue(string? uses)
        {
            if (uses == null) return null;
            var nodeKey = uses.Split('@')[0];
            if (nodeList.TryGetValue(nodeKey, out var value))
            {
                return value;
            }

            return uses;
        }

        foreach (var job in config.Jobs)
        {
            if (job is RocketSurgeonsGithubWorkflowJob workflowJob)
            {
                workflowJob.Uses = GetValue(workflowJob.Uses);
            }
            else if (job is RocketSurgeonsGithubActionsJob actionsJob)
            {
                foreach (var step in actionsJob.Steps.OfType<UsingStep>())
                {
                    step.Uses = step.Uses = GetValue(step.Uses);
                }
            }
        }
    }

    /// <inheritdoc />
    public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
    {
        return new(streamWriter, 2, "#");
    }

    /// <summary>
    ///     Gets the list of triggers as defined
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<GitHubActionsDetailedTrigger> GetTriggers(
        IEnumerable<GitHubActionsInput> inputs,
        IEnumerable<GitHubActionsWorkflowOutput> outputs,
        IEnumerable<GitHubActionsSecret> secrets
    )
    {
        if (On.Any(z => z == RocketSurgeonGitHubActionsTrigger.WorkflowDispatch))
        {
            yield return new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowDispatch,
                Inputs = inputs.ToList(),
            };
        }

        if (On.Any(z => z == RocketSurgeonGitHubActionsTrigger.WorkflowCall))
        {
            yield return new RocketSurgeonGitHubActionsWorkflowTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.WorkflowCall,
                Secrets = GetAllSecrets(secrets, false).ToList(),
                Outputs = outputs.ToList(),
                Inputs = inputs.ToList(),
            };
        }

        if (OnPushBranches.Length > 0 || OnPushTags.Length > 0 || OnPushIncludePaths.Length > 0 || OnPushExcludePaths.Length > 0)
        {
            yield return new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.Push,
                Branches = OnPushBranches,
                Tags = OnPushTags,
                IncludePaths = OnPushIncludePaths,
                ExcludePaths = OnPushExcludePaths,
            };
        }

        if (OnPullRequestBranches.Length > 0 || OnPullRequestTags.Length > 0 || OnPullRequestIncludePaths.Length > 0 || OnPullRequestExcludePaths.Length > 0)
        {
            yield return new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.PullRequest,
                Branches = OnPullRequestBranches,
                Tags = OnPullRequestTags,
                IncludePaths = OnPullRequestIncludePaths,
                ExcludePaths = OnPullRequestExcludePaths,
            };
        }

        if (OnPullRequestTargetBranches.Length > 0
         || OnPullRequestTargetTags.Length > 0
         || OnPullRequestTargetIncludePaths.Length > 0
         || OnPullRequestTargetExcludePaths.Length > 0)
        {
            yield return new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = RocketSurgeonGitHubActionsTrigger.PullRequestTarget,
                Branches = OnPullRequestTargetBranches,
                Tags = OnPullRequestTargetTags,
                IncludePaths = OnPullRequestTargetIncludePaths,
                ExcludePaths = OnPullRequestTargetExcludePaths,
            };
        }

        if (OnCronSchedule != null)
            yield return new GitHubActionsScheduledTrigger { Cron = OnCronSchedule, };
    }

    /// <summary>
    ///     Get a list of secrets that need to be imported.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<GitHubActionsSecret> GetAllSecrets(IEnumerable<GitHubActionsSecret> secrets, bool githubToken = true)
    {
        if (githubToken)
            yield return new("GITHUB_TOKEN", "The default github actions token", Alias: "GithubToken");
        foreach (var secret in secrets)
        {
            yield return secret;
        }
    }
}


using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

#pragma warning disable CA1019
#pragma warning disable CA1308
#pragma warning disable CA1721
#pragma warning disable CA1813
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Define the tasks to run when creating the github actions file
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GitHubActionsStepsAttribute : ChainedConfigurationAttributeBase
{
    private readonly string _name;
    private readonly GitHubActionsImage[] _images;

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="image"></param>
    /// <param name="images"></param>
    public GitHubActionsStepsAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images
    )
    {
        _name = name;
        _images = new[] { image }.Concat(images).ToArray();
    }

    /// <inheritdoc />
    public override Type HostType { get; } = typeof(GitHubActions);

    /// <inheritdoc />
    public override string ConfigurationFile => NukeBuild.RootDirectory / ".github" / "workflows" / $"{_name}.yml";

    /// <summary>
    ///     The targets to invoke
    /// </summary>
    public string[] InvokedTargets { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The parameters to import
    /// </summary>
    public string[] Parameters { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public override string IdPostfix => _name;

    /// <inheritdoc />
    public override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };

    /// <inheritdoc />
    public override IEnumerable<string> RelevantTargetNames => InvokedTargets;
    // public override IEnumerable<string> IrrelevantTargetNames => new string[0];

    /// <summary>
    ///     The triggers
    /// </summary>
    public GitHubActionsTrigger[] On { get; set; } = Array.Empty<GitHubActionsTrigger>();

    /// <summary>
    ///     The branches to run for push
    /// </summary>
    public string[] OnPushBranches { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The tags to run for push
    /// </summary>
    public string[] OnPushTags { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The paths to include for pushes
    /// </summary>
    public string[] OnPushIncludePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The paths to exclude for pushes
    /// </summary>
    public string[] OnPushExcludePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The branches for pull requests
    /// </summary>
    public string[] OnPullRequestBranches { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The tags for pull requests
    /// </summary>
    public string[] OnPullRequestTags { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The paths to include for pull requests
    /// </summary>
    public string[] OnPullRequestIncludePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The paths to exclude for pull requests
    /// </summary>
    public string[] OnPullRequestExcludePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The schedule to run on
    /// </summary>
    public string? OnCronSchedule { get; set; }

    /// <summary>
    ///     The secrets to import from the actions environment
    /// </summary>
    public string[] ImportSecrets { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Import the github secret token as the given value
    /// </summary>
    public string? ImportGitHubTokenAs { get; set; }

    /// <summary>
    ///     A list of static methods that can be used for additional configurations
    /// </summary>
    public string[] Enhancements { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
    {
        return new(streamWriter, 2, "#");
    }

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(
        NukeBuild build,
        IReadOnlyCollection<ExecutableTarget> relevantTargets
    )
    {
        var steps = new List<GitHubActionsStep>
        {
            new CheckoutStep("Checkout"),
            // new SetupDotNetStep("Install .NET Core Sdk"),
        };


        var globalToolStep = new RunStep("Install Nuke Global Tool")
        {
            Run = "dotnet tool install -g Nuke.GlobalTool"
        };
        var dotnetTools = Path.Combine(NukeBuild.RootDirectory, ".config/dotnet-tools.json");
        var localTool = false;
        if (File.Exists(dotnetTools))
        {
            steps.Add(
                new RunStep("dotnet tool restore")
                {
                    Run = "dotnet tool restore"
                }
            );
            if (!File.ReadAllText(dotnetTools).Contains("\"nuke.globaltool\": {", StringComparison.OrdinalIgnoreCase))
            {
                steps.Add(globalToolStep);
            }
            else
            {
                localTool = true;
            }
        }
        else
        {
            steps.Add(globalToolStep);
        }

        var stepParameters = GetParameters(build)
                            .Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ env.{z.Name.ToUpperInvariant()} }}}}'")
                            .ToArray()
                            .JoinSpace();

        var lookupTable = new LookupTable<ExecutableTarget, ExecutableTarget[]>();
        foreach (var (execute, targets) in relevantTargets
                                          .Select(
                                               x => ( ExecutableTarget: x,
                                                      Targets: GetInvokedTargets(x, relevantTargets).ToArray() )
                                           )
                                          .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Targets.ToArray()))
        )
        {
            steps.Add(
                new RunStep(execute.Name.Humanize(LetterCasing.Title))
                {
                    Run =
                        $"{( localTool ? "dotnet nuke" : "nuke" )} {targets.Select(z => z.Name).JoinSpace()} --skip {stepParameters}"
                           .TrimEnd()
                }
            );
        }

        var config = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = _name,
            DetailedTriggers = GetTriggers().ToList(),
            Jobs = new List<RocketSurgeonsGithubActionsJob>
            {
                new("Build")
                {
                    Steps = steps,
                    Images = _images,
                }
            }
        };

        if (Enhancements.Any())
        {
            foreach (var method in Enhancements.Join(build.GetType().GetMethods(), z => z, z => z.Name, (_, e) => e))
            {
                config = method.IsStatic
                    ? method.Invoke(null, new object[] { config }) as RocketSurgeonGitHubActionsConfiguration ?? config
                    : method.Invoke(build, new object[] { config }) as RocketSurgeonGitHubActionsConfiguration
                   ?? config;
            }
        }

        // TODO: Try to add support for interface based enhancements?

        return config;
    }

    /// <summary>
    ///     Gets a list of the parameters from within the nuke build environment
    /// </summary>
    /// <param name="build"></param>
    /// <returns></returns>
    protected virtual IEnumerable<GithubActionsNukeParameter> GetParameters(NukeBuild build)
    {
        var parameters =
            build.GetType().GetMembers(
                      BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public |
                      BindingFlags.FlattenHierarchy
                  )
                 .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null);
        foreach (var parameter in parameters)
        {
            if (Parameters.Any(
                z => z.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase) || z.Equals(
                    parameter.GetCustomAttribute<ParameterAttribute>()?.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            ))
            {
                var value = parameter.GetValue(build);
                if (value is AbsolutePath)
                {
                    value = null;
                }

                yield return new GithubActionsNukeParameter
                {
                    Name = parameter.GetCustomAttribute<ParameterAttribute>()?.Name ?? parameter.Name,
                    Default = value?.ToString() ?? "",
                };
            }
        }
    }

    /// <summary>
    ///     Get a list of values that need to be imported.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<(string key, string value)> GetImports()
    {
        string GetSecretValue(string secret)
        {
            return $"${{{{ secrets.{secret} }}}}";
        }

        if (ImportGitHubTokenAs != null)
            yield return ( ImportGitHubTokenAs, GetSecretValue("GITHUB_TOKEN") );

        foreach (var secret in ImportSecrets)
            yield return ( secret, GetSecretValue(secret) );
    }

    /// <summary>
    ///     Gets the list of triggers as defined
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerable<GitHubActionsDetailedTrigger> GetTriggers()
    {
        if (OnPushBranches.Length > 0 ||
            OnPushTags.Length > 0 ||
            OnPushIncludePaths.Length > 0 ||
            OnPushExcludePaths.Length > 0)
        {
            yield return new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.Push,
                Branches = OnPushBranches,
                Tags = OnPushTags,
                IncludePaths = OnPushIncludePaths,
                ExcludePaths = OnPushExcludePaths
            };
        }

        if (OnPullRequestBranches.Length > 0 ||
            OnPullRequestTags.Length > 0 ||
            OnPullRequestIncludePaths.Length > 0 ||
            OnPullRequestExcludePaths.Length > 0)
        {
            yield return new RocketSurgeonGitHubActionsVcsTrigger
            {
                Kind = GitHubActionsTrigger.PullRequest,
                Branches = OnPullRequestBranches,
                Tags = OnPullRequestTags,
                IncludePaths = OnPullRequestIncludePaths,
                ExcludePaths = OnPullRequestExcludePaths
            };
        }

        if (OnCronSchedule != null)
            yield return new GitHubActionsScheduledTrigger { Cron = OnCronSchedule };
    }
}

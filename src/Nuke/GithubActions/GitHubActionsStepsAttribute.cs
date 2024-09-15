using System.Diagnostics;
using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using YamlDotNet.RepresentationModel;

#pragma warning disable CA1019, CA1308, CA1721, CA1813
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Define the tasks to run when creating the github actions file
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class GitHubActionsStepsAttribute : GithubActionsStepsAttributeBase
{
    private readonly string[] _images;
    private readonly bool _isGithubHosted;

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
    ) : base(name)
    {
        _images = Enumerable
                 .Concat(new[] { image }, images)
                 .Select(z => z.GetValue().Replace(".", "_", StringComparison.Ordinal))
                 .ToArray();
        _isGithubHosted = true;
    }

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="image"></param>
    /// <param name="images"></param>
    public GitHubActionsStepsAttribute(
        string name,
        string image,
        params string[] images
    ) : base(name)
    {
        _images = Enumerable.Concat(new[] { image }, images).ToArray();
    }

    /// <summary>
    ///     The targets to invoke
    /// </summary>
    public string[] InvokedTargets { get; set; } = [];

    /// <summary>
    ///     The parameters to import
    /// </summary>
    public string[] Parameters { get; set; } = [];

    /// <inheritdoc />
    public override string IdPostfix => Name;

    /// <inheritdoc />
    public override IEnumerable<AbsolutePath> GeneratedFiles => new[] { ConfigurationFile };

    /// <summary>
    ///     Determine if you always want to build the nuke project during the ci run
    /// </summary>
    public bool AlwaysBuildNukeProject { get; set; }

    /// <inheritdoc />
    public override IEnumerable<string> RelevantTargetNames => InvokedTargets;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    // public override IEnumerable<string> IrrelevantTargetNames => new string[0];

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var steps = new List<GitHubActionsStep>();


        var attributes = Build.GetType().GetCustomAttributes().OfType<TriggerValueAttribute>().ToArray();
        var environmentAttributes = Build
                                   .GetType()
                                   .GetCustomAttributes()
                                   .OfType<GitHubActionsEnvironmentVariableAttribute>()
                                   .Select(z => z.ToEnvironmentVariable())
                                   .Concat(GetParameters(Build).Select(z => new GitHubActionsEnvironmentVariable(z.Name, z.Default)))
                                   .DistinctBy(z => z.Name)
                                   .ToArray();
        var inputs = attributes
                    .OfType<GitHubActionsInputAttribute>()
                    .Select(z => z.ToInput())
                    .SelectMany(
                         z =>
                         {
                             return new[]
                             {
                                 new KeyValuePair<string, GitHubActionsInput>(z.Name, z),
                                 new KeyValuePair<string, GitHubActionsInput>(z.Alias ?? z.Name.Pascalize(), z),
                             };
                         }
                     )
                    .DistinctBy(z => z.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(z => z.Key, z => z.Value, StringComparer.OrdinalIgnoreCase);
        var secrets = attributes.OfType<GitHubActionsSecretAttribute>().Select(z => z.ToSecret()).ToArray();
        var onePasswordSecrets = attributes.OfType<OnePasswordSecretAttribute>().Select(z => z.ToSecret()).ToArray();
        var onePasswordConnectServerSecrets = onePasswordSecrets
                                             .OfType<OnePasswordConnectServerSecret>()
                                             .Concat(attributes.OfType<OnePasswordConnectServerSecretAttribute>().Select(z => z.ToSecret()))
                                             .ToArray();
        var onePasswordServiceAccountSecrets = onePasswordSecrets
                                              .OfType<OnePasswordServiceAccountSecret>()
                                              .Concat(attributes.OfType<OnePasswordServiceAccountSecretAttribute>().Select(z => z.ToSecret()))
                                              .ToArray();
        var variables = attributes.OfType<GitHubActionsVariableAttribute>().Select(z => z.ToVariable()).ToArray();

        if (onePasswordServiceAccountSecrets.Any())
        {
            secrets =
            [
                .. secrets,
                .. onePasswordServiceAccountSecrets
                  .Select(z => z.Secret)
                  .Distinct()
                  .Select(z => new GitHubActionsSecret(z)),
            ];

            variables =
            [
                .. variables,
                .. onePasswordServiceAccountSecrets
                  .DistinctBy(z => z.Variable)
                  .Where(z => !z.Path.StartsWith("op://") && !string.IsNullOrWhiteSpace(z.Variable))
                  // ReSharper disable once NullableWarningSuppressionIsUsed
                  .Select(s => new GitHubActionsVariable(s.Variable!)),
            ];

            steps.AddRange(
                onePasswordServiceAccountSecrets
                   .GroupBy(z => z.Secret)
                   .Select(
                        static secrets => new UsingStep($"Load 1Password Secrets ({secrets.Key})")
                        {
                            Id = secrets.First().OutputId,
                            Uses = "1password/load-secrets-action@v1",
                            Outputs = secrets
                                     .Select(secret => new GitHubActionsOutput(secret.Name, secret.Description))
                                     .ToList(),
                            With = new() { ["export-env"] = "false" },
                            Environment = Enumerable
                                         .Concat(
                                              secrets
                                                 .Select(
                                                      z => new KeyValuePair<string, string>(
                                                          z.Name,
                                                          ( string.IsNullOrWhiteSpace(z.Variable) )
                                                              ? $"{z.Path}"
                                                              : $$$"""${{ vars.{{{z.Variable}}} }}/{{{z.Path.TrimStart('/')}}}"""
                                                      )
                                                  ),
                                              [
                                                  new(
                                                      "OP_SERVICE_ACCOUNT_TOKEN",
                                                      $$$"""${{ secrets.{{{secrets.First().OutputId}}} }}"""
                                                  ),
                                              ]
                                          )
                                         .ToDictionary(z => z.Key, z => z.Value),
                        }
                    )
            );
        }

        if (onePasswordConnectServerSecrets.Any())
        {
            secrets =
            [
                .. secrets,
                .. onePasswordConnectServerSecrets
                  .Select(z => z.ConnectToken)
                  .Distinct()
                  .Select(z => new GitHubActionsSecret(z)),
            ];

            variables =
            [
                .. variables,
                .. onePasswordConnectServerSecrets
                  .Select(z => z.ConnectHost)
                  .Distinct()
                  .Select(z => new GitHubActionsVariable(z)),
                .. onePasswordConnectServerSecrets
                  .DistinctBy(z => z.Variable)
                  .Where(z => !z.Path.StartsWith("op://") && !string.IsNullOrWhiteSpace(z.Variable))
                  // ReSharper disable once NullableWarningSuppressionIsUsed
                  .Select(s => new GitHubActionsVariable(s.Variable!)),
            ];

            steps.AddRange(
                onePasswordConnectServerSecrets
                   .GroupBy(z => $"{z.ConnectHost}, {z.ConnectToken}")
                   .Select(
                        static secrets => new UsingStep($"Load 1Password Secrets ({secrets.Key})")
                        {
                            Id = secrets.First().OutputId,
                            Uses = "1password/load-secrets-action@v1",
                            Outputs = secrets
                                     .Select(secret => new GitHubActionsOutput(secret.Name, secret.Description))
                                     .ToList(),
                            With = new() { ["export-env"] = "false" },
                            Environment = Enumerable
                                         .Concat(
                                              secrets
                                                 .Select(
                                                      z => new KeyValuePair<string, string>(
                                                          z.Name,
                                                          ( string.IsNullOrWhiteSpace(z.Variable) )
                                                              ? $"{z.Path}"
                                                              : $$$"""${{ vars.{{{z.Variable}}} }}/{{{z.Path.TrimStart('/')}}}"""
                                                      )
                                                  ),
                                              [
                                                  new(
                                                      "OP_CONNECT_HOST",
                                                      $$$"""${{ vars.{{{secrets.First().ConnectHost}}} }}"""
                                                  ),
                                                  new(
                                                      "OP_CONNECT_TOKEN",
                                                      $$$"""${{ secrets.{{{secrets.First().ConnectToken}}} }}"""
                                                  ),
                                              ]
                                          )
                                         .ToDictionary(z => z.Key, z => z.Value),
                        }
                    )
            );
        }

        steps.Add(new CheckoutStep("Checkout"));

        var globalToolStep = new RunStep("Install Nuke Global Tool")
        {
            Run = "dotnet tool install -g Nuke.GlobalTool"
        };
        // TODO: Add configuration to disable this?
        steps.Add(
            new RunStep("dotnet workload restore")
            {
                Run = "dotnet workload restore",
                ContinueOnError = true,
            }
        );
        var dotnetTools = Path.Combine(NukeBuild.RootDirectory, ".config/dotnet-tools.json");
        if (File.Exists(dotnetTools))
        {
            steps.Add(
                new RunStep("dotnet tool restore")
                {
                    Run = "dotnet tool restore"
                }
            );
        }

        var localTool = DotNetTool.IsInstalled("nuke");
        if (!localTool)
        {
            steps.Add(globalToolStep);
        }

        var environmentVariables =
            Enumerable
               .Concat(
                    GetAllSecrets(secrets)
                       // ReSharper disable CoVariantArrayConversion
                       .Concat<ITriggerValue>(variables)
                       .Concat(onePasswordConnectServerSecrets)
                       .Concat(onePasswordServiceAccountSecrets),
                    environmentAttributes
                )
               // ReSharper enable CoVariantArrayConversion
               .SelectMany(
                    z =>
                    {
                        return new[]
                        {
                            new KeyValuePair<string, ITriggerValue>(z.Name, z),
                            new KeyValuePair<string, ITriggerValue>(z.Alias ?? z.Name.Pascalize(), z),
                        };
                    }
                )
               .DistinctBy(z => z.Key, StringComparer.OrdinalIgnoreCase)
               .ToDictionary(z => z.Key, z => z.Value, StringComparer.OrdinalIgnoreCase);

        var implicitParameters =
            Build
               .GetType()
               .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
               .Where(x => x.GetCustomAttribute<ParameterAttribute>() is { });

        var stepParameters = new List<KeyValuePair<string, string>>();
        foreach (var par in implicitParameters)
        {
            var key = par.GetCustomAttribute<ParameterAttribute>()?.Name ?? par.Name;
            if (!environmentVariables.TryGetValue(key, out var value) || value is GitHubActionsInput)
            {
                continue;
            }

            var name = ( string.IsNullOrWhiteSpace(value.Prefix) ) ? value.Name : $"{value.Prefix}.{value.Name}";
            stepParameters.Add(new(key, $"{name}{( ( string.IsNullOrWhiteSpace(value.Default) ) ? "" : $" || {value.Default}" )}"));
        }

        var jobOutputs = new List<GitHubActionsStepOutput>();
        var requiredInputs = new List<GitHubActionsInput>();
        var lookupTable = new LookupTable<ExecutableTarget, ExecutableTarget[]>();
        var initialArguments = localTool ? new Arguments().Add("dotnet").Add("nuke") : new Arguments().Add("nuke");
        foreach ((var execute, var targets) in relevantTargets
                                                .Select(
                                                     x => (ExecutableTarget: x,
                                                            Targets: GetInvokedTargets(x, relevantTargets).ToArray())
                                                 )
                                                .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, [.. x.Targets,]))
                )
        {
            var localStepParameters = stepParameters.ToList();

            foreach (var target in targets)
            {
                var stepOutputs = GithubActionsExtensions.GetGithubActionsOutput(target);
                jobOutputs.AddRange(stepOutputs.Select(z => z.ToStep(execute.Name.Camelize())));

                foreach (var par in target
                                   .DelegateRequirements
                                   .Select(z => z.GetMemberInfo())
                                   .Where(z => z.GetCustomAttribute<ParameterAttribute>() is { }))
                {
                    var key = par.GetCustomAttribute<ParameterAttribute>()?.Name ?? par.Name;
                    if (inputs.TryGetValue(key, out var value))
                    {
                        requiredInputs.Add(value);
                        localStepParameters.Insert(
                            0,
                            new(
                                key,
                                $"{value.Prefix}.{value.Name}{( ( string.IsNullOrWhiteSpace(value.Default) ) ? "" : $" || {value.Default}" )}"
                            )
                        );
                    }
                }
            }

            var arguments = new Arguments()
                           .Concatenate(initialArguments)
                           .Add("--target {value}", targets.Select(z => z.Name), ' ')
                           .Add(
                                "--skip {value}",
                                GetInvokedTargets(execute, targets).SelectMany(GetTargetDependencies).Except(targets).Select(z => z.Name),
                                ' '
                            )
                           .Add(
                                "--{value}",
                                localStepParameters
                                   .GroupBy(z => z.Key, StringComparer.OrdinalIgnoreCase)
                                   .Select(z => z.Last())
                                   .ToDictionary(z => z.Key, z => $$$"""${{ {{{z.Value}}} }}"""),
                                "{key} {value}"
                            );

            if (!AlwaysBuildNukeProject)
            {
                initialArguments = new Arguments()
                                  .Add("dotnet")
                                  .Add(
                                       NukeBuild
                                          .RootDirectory.GetRelativePathTo(Assembly.GetEntryAssembly()?.Location)
                                          .ToUnixRelativePath()
                                   );
            }

            steps.Add(
                new RunStep(execute.Name.Humanize(LetterCasing.Title))
                {
                    Id = execute.Name.Camelize(),
                    Run = arguments.RenderForExecution(),
                }
            );
        }

        var outputs = jobOutputs.Select(z => z.ToWorkflow(Settings.DefaultGithubJobName));

        var buildJob = new RocketSurgeonsGithubActionsJob(Settings.DefaultGithubJobName)
        {
            Steps = steps,
            Outputs = jobOutputs,
            RunsOn = ( !_isGithubHosted ) ? _images : [],
            Matrix = _isGithubHosted ? _images : [],
            // TODO: Figure out what this looks like here
            //                    Environment = inputs
            //                                 .Concat<ITriggerValue>(GetAllSecrets(secrets))
            //                                 .Concat(variables)
            //                                 .Select(
            //                                      z => new KeyValuePair<string, string>(
            //                                          $"{( z.Prefix.Equals("ENV", StringComparison.OrdinalIgnoreCase) ? "" : $"{z.Prefix.ToUpperInvariant()}_" )}{( z.Alias ?? z.Name ).ToUpperInvariant()}",
            //                                          $$$"""${{ {{{z.Prefix}}}.{{{z.Name}}} }}"""
            //                                      )
            //                                  )
            //                                 .ToDictionary(z => z.Key, z => z.Value, StringComparer.OrdinalIgnoreCase)
        };
        // ReSharper disable once PossibleMultipleEnumeration
        var triggers = GetTriggers(requiredInputs, outputs, secrets).ToArray();

        var config = new RocketSurgeonGitHubActionsConfiguration
        {
            Name = Name,
            DetailedTriggers = [.. triggers,],
            // TODO: Figure out what this looks like here
            //            Environment = environmentAttributes
            Jobs = [buildJob,],
        };

        ApplyEnhancements(config);

        if (!buildJob.Name.Equals(Settings.DefaultGithubJobName, StringComparison.OrdinalIgnoreCase))
        {
            // ReSharper disable once PossibleMultipleEnumeration
            config.DetailedTriggers = GetTriggers(requiredInputs, outputs, secrets)
                                     .Concat(config.DetailedTriggers.Except(triggers))
                                     .ToList();
        }

        // need a better way to do this more generically
        if (buildJob.Steps.OfType<UsingStep>().Any(z => z.Uses?.StartsWith("codecov/codecov-action", StringComparison.OrdinalIgnoreCase) == true))
        {
            foreach (var trigger in config.DetailedTriggers.OfType<RocketSurgeonGitHubActionsWorkflowTrigger>())
            {
                if (trigger.Secrets.Any(s => s.Name == "CODECOV_TOKEN"))
                {
                    continue;
                }

                trigger.Secrets.Add(new("CODECOV_TOKEN", "The codecov token", Alias: "CodecovToken"));
            }
        }

        if (_isGithubHosted && _images is { Length: > 1, })
        {
            var mainOs = _images.First();
            foreach (var step in steps.OfType<UploadArtifactStep>().ToList())
            {
                steps.Insert(
                    steps.IndexOf(step),
                    new UploadArtifactStep($$$"""{{{step.StepName}}} (${{ matrix.os }})""")
                    {
                        If = ( step.If is { } ) ? $"{step.If} && matrix.os != '{mainOs}'" : $"matrix.os != '{mainOs}'",
                        Name = $$$"""${{ matrix.os }}-{{{step.Name}}}""",
                        Path = step.Path,
                        Environment = step.Environment.ToDictionary(z => z.Key, z => z.Value),
                        Outputs = [.. step.Outputs,],
                        With = step.With.ToDictionary(z => z.Key, z => z.Value),
                        Uses = step.Uses,
                        Overwrite = step.Overwrite,
                        CompressionLevel = step.CompressionLevel,
                        ContinueOnError = step.ContinueOnError,
                        RetentionDays = step.RetentionDays,
                        IfNoFilesFound = step.IfNoFilesFound,
                    }
                );
                step.If = ( step.If is { } ) ? $"{step.If} && matrix.os == '{mainOs}'" : $"matrix.os == '{mainOs}'";
            }
        }

        // This will normalize the version numbers against the existing file.
        if (!File.Exists(ConfigurationFile))
        {
            return config;
        }

        NormalizeActionVersions(config);
        return config;
    }

    /// <summary>
    ///     Method will ensure the versions defined in the actions are normalized to the same version that currently exists to allow renovate to upgrade actions.
    /// </summary>
    /// <param name="config"></param>
    protected void NormalizeActionVersions(RocketSurgeonGitHubActionsConfiguration config)
    {
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
                           z => (name: ( (YamlScalarNode)z.Children[key] ).Value!.Split("@")[0],
                                  value: ( (YamlScalarNode)z.Children[key] ).Value)
                       )
                      .DistinctBy(z => z.name)
                      .ToDictionary(
                           z => z.name,
                           z => z.value
                       );

        string? GetValue(string? uses)
        {
            if (uses is null)
            {
                return null;
            }

            var nodeKey = uses.Split('@')[0];
            return ( nodeList.TryGetValue(nodeKey, out var value) ) ? value : uses;
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

    /// <summary>
    ///     Gets a list of the parameters from within the nuke build environment
    /// </summary>
    /// <param name="build"></param>
    /// <returns></returns>
    protected virtual IEnumerable<GithubActionsNukeParameter> GetParameters(INukeBuild build)
    {
        var parameters =
            build
               .GetType()
               .GetMembers(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                )
               .Where(x => x.GetCustomAttribute<ParameterAttribute>() is { });
        foreach (var parameter in parameters)
        {
            if (Parameters.Any(
                    z => z.Equals(parameter.Name, StringComparison.OrdinalIgnoreCase)
                     || z.Equals(
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

                yield return new()
                {
                    Name = parameter.GetCustomAttribute<ParameterAttribute>()?.Name ?? parameter.Name,
                    Default = value?.ToString() ?? "",
                };
            }
        }
    }
}

//[PublicAPI]
//[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
//public abstract class GitHubActionsWorkflowAttribute : GithubActionsStepsAttributeBase
//{
//    public GitHubActionsWorkflowAttribute(string name) : base(name)
//    {
//    }
//
//    public sealed override ConfigurationEntity GetConfiguration(
//        NukeBuild build, IReadOnlyCollection<ExecutableTarget> relevantTargets
//    )
//    {
//        var config = new RocketSurgeonGitHubActionsConfiguration()
//        {
//            Jobs = new List<RocketSurgeonsGithubActionsJobBase>()
//            {
//                new RocketSurgeonsGithubWorkflowJob(_name)
//                {
//                    Secrets = Secrets,
//                    Uses = Uses,
//                    With = With
//                }
//            }
//        };
//        ApplyEnhancements(build, config);
//        return config;
//    }
//
//    /// <inheritdoc />
//    public sealed override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
//
//    /// <inheritdoc />
//    public sealed override IEnumerable<string> RelevantTargetNames => Array.Empty<string>();
//
//    /// <summary>
//    ///     The action to use.
//    /// </summary>
//    public string? Uses { get; set; }
//
//    /// <summary>
//    ///     The properties to use with the action
//    /// </summary>
//    public Dictionary<string, string> With { get; set; } = new(StringComparer.OrdinalIgnoreCase);
//
//    /// <summary>
//    ///     The properties to use with the action
//    /// </summary>
//    public Dictionary<string, string> Secrets { get; set; } = new(StringComparer.OrdinalIgnoreCase);
//}
//
//public class CloseMilestoneWorkflowAttribute : GitHubActionsWorkflowAttribute
//{
//    public CloseMilestoneWorkflowAttribute(string name) : base(name)
//    {
//
//        Uses = "RocketSurgeonsGuild/actions/.github/workflows/close-milestone6.yml@v0.3.8";
//        Secrets = new Dictionary<string, string>()
//        {
//            ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
//            ["RSG_BOT_TOKEN"] = "${{ secrets.RSG_BOT_TOKEN }}",
//        };
//    }
//}

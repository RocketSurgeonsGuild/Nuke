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
        _images = new[] { image, }
                 .Concat(images)
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
        _images = new[] { image, }.Concat(images).ToArray();
    }

    /// <summary>
    ///     The targets to invoke
    /// </summary>
    public string[] InvokedTargets { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     The parameters to import
    /// </summary>
    public string[] Parameters { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public override string IdPostfix => Name;

    /// <inheritdoc />
    public override IEnumerable<AbsolutePath> GeneratedFiles => new[] { ConfigurationFile, };

    /// <inheritdoc />
    public override IEnumerable<string> RelevantTargetNames => InvokedTargets;
    // public override IEnumerable<string> IrrelevantTargetNames => new string[0];

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var steps = new List<GitHubActionsStep>
                    {
                        new CheckoutStep("Checkout"),
                        // new SetupDotNetStep("Install .NET Core Sdk"),
                    };


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
            secrets = secrets
                     .Concat(
                          onePasswordServiceAccountSecrets
                             .Select(z => z.Secret ?? "OP_SERVICE_ACCOUNT_TOKEN")
                             .Distinct()
                             .Select(z => new GitHubActionsSecret(z))
                      )
                     .ToArray();

            variables = variables
                       .Concat(
                            onePasswordServiceAccountSecrets
                               .DistinctBy(z => z.Variable)
                               .Where(z => !z.Path.StartsWith("op://") && !string.IsNullOrWhiteSpace(z.Variable))
                               .Select(s => new GitHubActionsVariable(s.Variable!))
                        )
                       .ToArray();

            steps.AddRange(
                onePasswordServiceAccountSecrets
                   .GroupBy(z => z.Secret ?? "OP_SERVICE_ACCOUNT_TOKEN")
                   .Select(
                        static secrets => new UsingStep($"Load 1Password Secrets ({secrets.Key})")
                                          {
                                              Id = "1password",
                                              Uses = "1password/load-secrets-action@v1",
                                              Outputs = secrets
                                                       .Select(secret => new GitHubActionsOutput(secret.Name, secret.Description))
                                                       .ToList(),
                                              Environment = secrets
                                                           .Select(
                                                                z => new KeyValuePair<string, string>(
                                                                    z.Name,
                                                                    string.IsNullOrWhiteSpace(z.Variable)
                                                                        ? $$$"""{{{z.Path}}}"""
                                                                        : $$$"""${{ vars.{{{z.Variable}}} }}/{{{z.Path.TrimStart('/')}}}"""
                                                                )
                                                            )
                                                           .Concat(
                                                                [
                                                                    new KeyValuePair<string, string>(
                                                                        "OP_SERVICE_ACCOUNT_TOKEN",
                                                                        $$$"""${{ secrets.{{{secrets.Key}}} }}"""
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
            secrets = secrets
                     .Concat(
                          onePasswordConnectServerSecrets
                             .Select(z => z.ConnectToken ?? "OP_CONNECT_TOKEN")
                             .Distinct()
                             .Select(z => new GitHubActionsSecret(z))
                      )
                     .ToArray();

            variables = variables
                       .Concat(
                            onePasswordConnectServerSecrets
                               .Select(z => z.ConnectHost ?? "OP_CONNECT_HOST")
                               .Distinct()
                               .Select(z => new GitHubActionsVariable(z))
                        )
                       .Concat(
                            onePasswordConnectServerSecrets
                               .DistinctBy(z => z.Variable)
                               .Where(z => !z.Path.StartsWith("op://") && !string.IsNullOrWhiteSpace(z.Variable))
                               .Select(s => new GitHubActionsVariable(s.Variable!))
                        )
                       .ToArray();

            steps.AddRange(
                onePasswordConnectServerSecrets
                   .GroupBy(z => ( Host: z.ConnectHost ?? "OP_CONNECT_HOST", Token: z.ConnectToken ?? "OP_CONNECT_TOKEN" ))
                   .Select(
                        static secrets => new UsingStep($"Load 1Password Secrets {secrets.Key}")
                                          {
                                              Id = "1password",
                                              Uses = "1password/load-secrets-action@v1",
                                              Outputs = secrets
                                                       .Select(secret => new GitHubActionsOutput(secret.Name, secret.Description))
                                                       .ToList(),
                                              Environment = secrets
                                                           .Select(
                                                                z => new KeyValuePair<string, string>(
                                                                    z.Name,
                                                                    string.IsNullOrWhiteSpace(z.Variable)
                                                                        ? $$$"""{{{z.Path}}}"""
                                                                        : $$$"""${{ vars.{{{z.Variable}}} }}/{{{z.Path.TrimStart('/')}}}"""
                                                                )
                                                            )
                                                           .Concat(
                                                                [
                                                                    new(
                                                                        "OP_CONNECT_HOST",
                                                                        $$$"""${{ vars.{{{secrets.Key.Host}}} }}"""
                                                                    ),
                                                                    new KeyValuePair<string, string>(
                                                                        "OP_CONNECT_TOKEN",
                                                                        $$$"""${{ secrets.{{{secrets.Key.Token}}} }}"""
                                                                    ),
                                                                ]
                                                            )
                                                           .ToDictionary(z => z.Key, z => z.Value),
                                          }
                    )
            );
        }

        var globalToolStep = new RunStep("Install Nuke Global Tool")
                             {
                                 Run = "dotnet tool install -g Nuke.GlobalTool",
                             };
        var dotnetTools = Path.Combine(NukeBuild.RootDirectory, ".config/dotnet-tools.json");
        var localTool = false;
        if (File.Exists(dotnetTools))
        {
            steps.Add(
                new RunStep("dotnet tool restore")
                {
                    Run = "dotnet tool restore",
                }
            );
            if (!DotNetTool.IsInstalled("nuke"))
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

        var environmentVariables =
            GetAllSecrets(secrets)
                // ReSharper disable once CoVariantArrayConversion
               .Concat<ITriggerValue>(variables)
               .Concat(onePasswordSecrets)
                // ReSharper disable once CoVariantArrayConversion
               .Concat(environmentAttributes)
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
               .GetMembers(
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                )
               .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null);

        var stepParameters = new List<KeyValuePair<string, string>>();
        foreach (var par in implicitParameters)
        {
            var key = par.GetCustomAttribute<ParameterAttribute>()?.Name ?? par.Name;
            if (environmentVariables.TryGetValue(key, out var value) && value is not GitHubActionsInput)
            {
                //                Log.Logger.Information("Found Parameter {Name}", value.Name);
                var name = string.IsNullOrWhiteSpace(value.Prefix) ? value.Name : $"{value.Prefix}.{value.Name}";
                stepParameters.Add(new(key, $"{name}{( string.IsNullOrWhiteSpace(value.Default) ? "" : $" || {value.Default}" )}"));
            }
        }

        var jobOutputs = new List<GitHubActionsStepOutput>();
        var requiredInputs = new List<GitHubActionsInput>();
        var lookupTable = new LookupTable<ExecutableTarget, ExecutableTarget[]>();
        foreach (( var execute, var targets ) in relevantTargets
                                                .Select(
                                                     x => ( ExecutableTarget: x,
                                                            Targets: GetInvokedTargets(x, relevantTargets).ToArray() )
                                                 )
                                                .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Targets.ToArray()))
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
                                   .Where(z => z.GetCustomAttribute<ParameterAttribute>() != null))
                {
                    var key = par.GetCustomAttribute<ParameterAttribute>()?.Name ?? par.Name;
                    if (inputs.TryGetValue(key, out var value))
                    {
                        requiredInputs.Add(value);
                        localStepParameters.Insert(
                            0,
                            new(
                                key,
                                $"{value.Prefix}.{value.Name}{( string.IsNullOrWhiteSpace(value.Default) ? "" : $" || {value.Default}" )}"
                            )
                        );
                    }
                }
            }

            steps.Add(
                new RunStep(execute.Name.Humanize(LetterCasing.Title))
                {
                    Id = execute.Name.Camelize(),
                    Run =
                        $"{( localTool ? "dotnet nuke" : "nuke" )} {targets.Select(z => z.Name).JoinSpace()} --skip {localStepParameters
                           .GroupBy(z => z.Key, StringComparer.OrdinalIgnoreCase)
                           .Select(z => z.Last())
                           .Select(z => $$$"""--{{{z.Key.ToLowerInvariant()}}} '${{ {{{z.Value}}} }}'""").JoinSpace()}"
                           .TrimEnd(),
                }
            );
        }

        var outputs = jobOutputs.Select(z => z.ToWorkflow("build"));

        var buildJob = new RocketSurgeonsGithubActionsJob("build")
                       {
                           Steps = steps,
                           Outputs = jobOutputs,
                           RunsOn = !_isGithubHosted ? _images : Array.Empty<string>(),
                           Matrix = _isGithubHosted ? _images : Array.Empty<string>(),
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
                         DetailedTriggers = triggers.ToList(),
                         // TODO: Figure out what this looks like here
                         //            Environment = environmentAttributes
                         Jobs = new() { buildJob, },
                     };

        ApplyEnhancements(config);

        if (!buildJob.Name.Equals("build", StringComparison.OrdinalIgnoreCase))
        {
            // ReSharper disable once PossibleMultipleEnumeration
            config.DetailedTriggers = GetTriggers(requiredInputs, outputs, secrets)
                                     .Concat(config.DetailedTriggers.Except(triggers))
                                     .ToList();
        }

        return config;
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
               .Where(x => x.GetCustomAttribute<ParameterAttribute>() != null);
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
//        Uses = "RocketSurgeonsGuild/actions/.github/workflows/close-milestone.yml@v0.3.0";
//        Secrets = new Dictionary<string, string>()
//        {
//            ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
//            ["RSG_BOT_TOKEN"] = "${{ secrets.RSG_BOT_TOKEN }}",
//        };
//    }
//}
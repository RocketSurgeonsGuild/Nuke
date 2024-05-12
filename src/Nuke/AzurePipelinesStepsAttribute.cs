using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.Azp;

#pragma warning disable CA1813

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Define a set of azure pipelines steps
/// </summary>
[PublicAPI]
public class AzurePipelinesStepsAttribute : ChainedConfigurationAttributeBase
{
    private readonly Dictionary<string, string> _defaultSymbols = new()
    {
        ["Build"] = "⚙",
        ["Compile"] = "⚙",
        ["Test"] = "🚦",
        ["Pack"] = "📦",
        ["Restore"] = "📪",
        ["Publish"] = "🚢",
    };

    /// <inheritdoc />
    public override AbsolutePath ConfigurationFile => NukeBuild.RootDirectory / "azure-pipelines.nuke.yml";

    /// <inheritdoc />
    public override Type HostType => typeof(AzurePipelines);

    /// <inheritdoc />
    public override IEnumerable<AbsolutePath> GeneratedFiles => new[] { ConfigurationFile, };

    /// <inheritdoc />
    public override IEnumerable<string> RelevantTargetNames => InvokeTargets;

    /// <summary>
    ///     The targets to invoke
    /// </summary>
    public string[] InvokeTargets { get; set; } = [];

    /// <summary>
    ///     The parameters to be used
    /// </summary>
    public string[] Parameters { get; set; } = [];

    /// <inheritdoc />
    public override CustomFileWriter CreateWriter(StreamWriter streamWriter)
    {
        return new(streamWriter, 2, "#");
    }

    /// <inheritdoc />
    public override ConfigurationEntity GetConfiguration(
        IReadOnlyCollection<ExecutableTarget> relevantTargets
    )
    {
        var paramList = new List<AzurePipelinesParameter>();
        var parameters =
            Build
               .GetType()
               .GetInterfaces()
               .SelectMany(x => x.GetMembers())
               .Concat(
                    Build
                       .GetType()
                       .GetMembers(
                            BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy
                        )
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
                var value = parameter.GetValue(Build);
                if (value is AbsolutePath) value = null;

                paramList.Add(
                    new()
                    {
                        Name = parameter.GetCustomAttribute<ParameterAttribute>()?.Name ?? parameter.Name,
                        Default = value?.ToString() ?? "",
                    }
                );
            }
        }

        var lookupTable = new LookupTable<ExecutableTarget, AzurePipelinesStep>();
        var steps = relevantTargets
                   .Select(x => ( ExecutableTarget: x, Job: GetStep(x, relevantTargets, lookupTable) ))
                   .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Job))
                   .Select(x => x.Job)
                   .ToArray();

        return new AzurePipelinesSteps
        {
            Parameters = paramList.ToArray(),
            Steps = steps,
        };
    }

    /// <summary>
    ///     Get the step for the given targets
    /// </summary>
    /// <param name="executableTarget"></param>
    /// <param name="relevantTargets"></param>
    /// <param name="jobs"></param>
    /// <returns></returns>
    protected virtual AzurePipelinesStep GetStep(
        ExecutableTarget executableTarget,
        IReadOnlyCollection<ExecutableTarget> relevantTargets,
        LookupTable<ExecutableTarget, AzurePipelinesStep> jobs
    )
    {
        var chainLinkNames = GetInvokedTargets(executableTarget, relevantTargets).Select(z => z.Name).ToArray();
        var tool = DotnetTool.IsInstalled("codecov.tool") ? "dotnet nuke" : "nuke";

        return new()
        {
            Name = executableTarget.Name,
            DisplayName = GetStepName(executableTarget.Name),
            ScriptPath = tool,
            InvokedTargets = chainLinkNames,
        };
    }

    /// <summary>
    ///     Get the step name using the symbol if defined
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual string GetStepName(string name)
    {
        var symbol = _defaultSymbols.FirstOrDefault(z => z.Key.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                                    .Value;
        if (string.IsNullOrWhiteSpace(symbol)) return name;

        return $"{symbol} {name}";
    }
}

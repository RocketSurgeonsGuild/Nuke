using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using System.IO;
using Rocket.Surgery.Nuke.AzurePipelines;
#pragma warning disable 1591

namespace Rocket.Surgery.Nuke
{
    [PublicAPI]
    public class AzurePipelinesStepsAttribute : ChainedConfigurationAttributeBase
    {
        public override string ConfigurationFile => NukeBuild.RootDirectory / "azure-pipelines.nuke.yml";
        public override HostType HostType => HostType.AzurePipelines;
        public override IEnumerable<string> GeneratedFiles => new[] { ConfigurationFile };
        public override IEnumerable<string> RelevantTargetNames => InvokeTargets;

        public string[] InvokeTargets { get; set; } = Array.Empty<string>();
        public string[] Parameters { get; set; } = Array.Empty<string>();

        public override CustomFileWriter CreateWriter(StreamWriter writer) => new CustomFileWriter(writer, indentationFactor: 2, commentPrefix: "#");

        public override ConfigurationEntity GetConfiguration(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> relevantTargets
        )
        {
            var paramList = new List<AzurePipelinesParameter>();
            var parameters =
                build.GetType()
                   .GetInterfaces()
                   .SelectMany(x => x.GetMembers())
                   .Concat(build.GetType()
                       .GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
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

                    paramList.Add(
                        new AzurePipelinesParameter()
                        {
                            Name = parameter.GetCustomAttribute<ParameterAttribute>()?.Name ?? parameter.Name,
                            Default = value?.ToString() ?? "",
                        }
                    );
                }
            }

            var lookupTable = new LookupTable<ExecutableTarget, AzurePipelinesStep>();
            var steps = relevantTargets
               .Select(x => (ExecutableTarget: x, Job: GetStep(x, relevantTargets, lookupTable)))
               .ForEachLazy(x => lookupTable.Add(x.ExecutableTarget, x.Job))
               .Select(x => x.Job).ToArray();

            return new AzurePipelinesSteps()
            {
                Parameters = paramList.ToArray(),
                Steps = steps
            };
        }

        protected virtual AzurePipelinesStep GetStep(
            ExecutableTarget executableTarget,
            IReadOnlyCollection<ExecutableTarget> relevantTargets,
            LookupTable<ExecutableTarget, AzurePipelinesStep> jobs
        )
        {
            var chainLinkNames = GetInvokedTargets(executableTarget, relevantTargets).Select(z => z.Name).ToArray();
            return new AzurePipelinesStep
            {
                Name = executableTarget.Name,
                DisplayName = GetStepName(executableTarget.Name),
                ScriptPath = Path.ChangeExtension(NukeBuild.RootDirectory.GlobFiles("build.ps1", "build.sh")
                    .Select(x => NukeBuild.RootDirectory.GetUnixRelativePathTo(x))
                    .FirstOrDefault()
                    .NotNull("Must have a build script of build.ps1 or build.sh"), ".ps1"),
                InvokedTargets = chainLinkNames,
            };
        }

        private readonly Dictionary<string, string> _defaultSymbols = new Dictionary<string, string>()
        {
            ["Build"] = "⚙",
            ["Compile"] = "⚙",
            ["Test"] = "🚦",
            ["Pack"] = "📦",
            ["Restore"] = "📪",
            ["Publish"] = "🚢",
        };

        protected virtual string GetStepName(string name)
        {
            var symbol = _defaultSymbols.FirstOrDefault(z => z.Key.EndsWith(name, StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrWhiteSpace(symbol)) return name;

            return $"{symbol} {name}";
        }
    }
}
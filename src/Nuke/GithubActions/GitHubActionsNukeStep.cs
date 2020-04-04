using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GitHubActionsNukeSteps : GitHubActionsStep
    {
        public GithubActionsParameter[] Parameters { get; set; }
        public string Name { get; set; }
        public string ScriptPath { get; set; }
        public Dictionary<string, string> Imports { get; set; }
        public (ExecutableTarget ExecutableTarget, string[] Targets)[] InvokedTargets { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            var parameters = Parameters.Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ env.{z.Name.ToUpperInvariant()} }}}}'")
               .ToArray()
               .JoinSpace();


            foreach (var (executableTarget, targets) in InvokedTargets)
            {
                writer.WriteLine($"- name: '{GetStepName(executableTarget.Name)}'");
                writer.WriteLine($"  shell: pwsh");
                writer.WriteLine($"  run: ./{ScriptPath} {targets.JoinSpace()} --skip {parameters}".TrimEnd());

                if (Imports.Count > 0)
                {
                    using (writer.Indent())
                    {
                        writer.WriteLine("env:");
                        using (writer.Indent())
                        {
                            Imports.ForEach(x => writer.WriteLine($"  {x.Key}: {x.Value}"));
                        }
                    }
                }
            }
        }

        private readonly Dictionary<string, string> _defaultSymbols = new Dictionary<string, string>()
        {
            ["Build"] = "⚙",
            ["Compile"] = "⚙",
            ["Test"] = "🚦",
            ["Pack"] = "📦",
            ["Restore"] = "📪",
            ["DotnetToolRestore"] = "🛠",
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
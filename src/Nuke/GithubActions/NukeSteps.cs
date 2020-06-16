using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class NukeSteps : RunStep
    {
        public NukeSteps(string name) : base(name)
        {
            Shell = GithubActionShell.Pwsh;
        }

        public GithubActionsNukeParameter[] Parameters { get; set; }
        public string ScriptPath { get; set; }
        public (ExecutableTarget ExecutableTarget, string[] Targets)[] InvokedTargets { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            var parameters = Parameters.Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ env.{z.Name.ToUpperInvariant()} }}}}'")
               .ToArray()
               .JoinSpace();

            foreach (var (executableTarget, targets) in InvokedTargets)
            {
                Run = $"./{ScriptPath} {targets.JoinSpace()} --skip {parameters}".TrimEnd();
                base.Write(writer);
            }
        }
    }
}
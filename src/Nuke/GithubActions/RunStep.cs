using System;
using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{

    public class RunStep : BaseGitHubActionsStep
    {
        public RunStep(string name) : base(name)
        {
        }

        public string Run { get; set; }
        public GithubActionShell Shell { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            base.Write(writer);
            using (writer.Indent())
            {
                if (!string.IsNullOrWhiteSpace(Shell))
                    writer.WriteLine($"shell: {Shell}");
                writer.WriteLine($"run: |");
                using (writer.Indent())
                {
                    foreach (var line in Run.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        writer.WriteLine(line.Trim());
                    }
                }
            }
        }
    }
}
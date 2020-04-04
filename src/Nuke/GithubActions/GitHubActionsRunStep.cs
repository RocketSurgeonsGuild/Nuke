using System.Collections.Generic;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GitHubActionsRunStep : GitHubActionsStep
    {
        public string Command { get; set; }
        public Dictionary<string, string> Imports { get; set; } = new Dictionary<string, string>();

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {Name}");
            if (!string.IsNullOrWhiteSpace(Id))
            {
                writer.WriteLine($"  id: {Id}");
            }
            writer.WriteLine($"  run: {Command}");

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
}
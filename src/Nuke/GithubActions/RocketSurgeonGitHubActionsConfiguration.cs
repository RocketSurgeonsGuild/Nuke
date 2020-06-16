using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class RocketSurgeonGitHubActionsConfiguration : ConfigurationEntity
    {
        public string Name { get; set; }
        public List<GitHubActionsTrigger> ShortTriggers { get; set; } = new List<GitHubActionsTrigger>();
        public List<GitHubActionsDetailedTrigger> DetailedTriggers { get; set; } = new List<GitHubActionsDetailedTrigger>();
        public List<RocketSurgeonsGithubActionsJob> Jobs { get; set; } = new List<RocketSurgeonsGithubActionsJob>();
        public Dictionary<string, string> Environment { get; set; } = new Dictionary<string, string>();

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"name: {Name}");
            writer.WriteLine();

            if (ShortTriggers.Count > 0)
            {
                writer.WriteLine($"on: [{ShortTriggers.Select(x => x.GetValue().ToLowerInvariant()).JoinComma()}]");
            }
            else
            {
                writer.WriteLine("on:");
                using (writer.Indent())
                {
                    DetailedTriggers.ForEach(x => x.Write(writer));
                }
            }

            writer.WriteLine();

            writer.WriteLine("jobs:");
            using (writer.Indent())
            {
                Jobs.ForEach(x => x.Write(writer));
            }
            if (Environment?.Any() == true)
            {
                writer.WriteLine("env:");
                using (writer.Indent())
                {
                    Environment.ForEach(x => writer.WriteLine($"{x.Key}: {x.Value}"));
                }
            }
        }
    }
}
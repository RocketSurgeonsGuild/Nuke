using System;
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
    public class GitHubActionsConfiguration : ConfigurationEntity
    {
        public string Name { get; set; }

        public GitHubActionsTrigger[] ShortTriggers { get; set; } = Array.Empty<GitHubActionsTrigger>();
        public GitHubActionsDetailedTrigger[] DetailedTriggers { get; set; }
        public GitHubActionsJob[] Jobs { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"name: {Name}");
            writer.WriteLine();

            if (ShortTriggers.Length > 0)
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
        }
    }
}
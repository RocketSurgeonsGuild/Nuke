using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class RocketSurgeonsGithubActionsJob : ConfigurationEntity
    {
        public RocketSurgeonsGithubActionsJob(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }
        public string Name { get; }
        public IEnumerable<GitHubActionsImage> Images { get; set; }
        public GithubActionCondition If { get; set; }
        public List<GitHubActionsStep> Steps { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"{Name}:");

            using (writer.Indent())
            {
                writer.WriteLine($"strategy:");
                using (writer.Indent())
                {
                    writer.WriteLine($"fail-fast: false");
                    writer.WriteLine($"matrix:");

                    using (writer.Indent())
                    {
                        var images = string.Join(", ", Images.Select(image => image.GetValue().Replace(".", "_")));
                        writer.WriteLine($"os: [{images}]");
                    }
                }

                writer.WriteLine($"runs-on: ${{{{ matrix.os }}}}");
                writer.WriteLine("steps:");
                using (writer.Indent())
                {
                    foreach (var step in Steps)
                    {
                        step.Write(writer);
                    }
                }
            }
        }
    }
}
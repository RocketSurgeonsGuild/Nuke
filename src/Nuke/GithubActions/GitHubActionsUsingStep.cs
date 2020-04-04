using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GitHubActionsUsingStep : GitHubActionsStep
    {
        public string Using { get; set; }
        public Dictionary<string, string> With { get; set; } = new Dictionary<string, string>();

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine($"- name: {Name}");
            if (!string.IsNullOrWhiteSpace(Id))
            {
                writer.WriteLine($"  id: {Id}");
            }
            writer.WriteLine($"  uses: {Using}");

            if (With.Any())
            {
                using (writer.Indent())
                {
                    writer.WriteLine("with:");
                    using (writer.Indent())
                    {
                        foreach (var item in With)
                            writer.WriteLine($"{item.Key}: '{item.Value}'");
                    }
                }
            }
        }
    }
}
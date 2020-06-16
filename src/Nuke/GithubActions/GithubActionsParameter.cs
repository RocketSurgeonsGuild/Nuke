using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public class GithubActionsNukeParameter : ConfigurationEntity
    {
        public string Name { get; set; }
        public string Default { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using var a = writer.WriteBlock($"{Name}: '{Default}'");
        }
    }

}
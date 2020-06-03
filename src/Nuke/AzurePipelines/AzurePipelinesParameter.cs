using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;
#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.AzurePipelines
{
    public class AzurePipelinesParameter : ConfigurationEntity
    {
        public string Name { get; set; }
        public string Default { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using var a = writer.WriteBlock($"{Name}: '{Default}'");
        }
    }
}
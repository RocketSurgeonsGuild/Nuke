using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;

#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.Azp
{
    public class AzurePipelinesParameter : ConfigurationEntity
    {
        public string Name { get; set; } = null!;
        public string Default { get; set; } = null!;

        public override void Write(CustomFileWriter writer)
        {
            using var a = writer.WriteBlock($"{Name}: '{Default}'");
        }
    }
}
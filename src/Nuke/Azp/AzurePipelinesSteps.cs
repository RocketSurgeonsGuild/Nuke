using System;
using System.Linq;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;

#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.Azp
{
    public class AzurePipelinesSteps : ConfigurationEntity
    {
        public AzurePipelinesParameter[] Parameters { get; set; } = Array.Empty<AzurePipelinesParameter>();
        public AzurePipelinesStep[] Steps { get; set; } = Array.Empty<AzurePipelinesStep>();

        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("#");
            if (Parameters.Length > 0)
            {
                using (writer.WriteBlock("parameters:"))
                {
                    foreach (var item in Parameters)
                    {
                        item.Write(writer);
                    }
                }
            }

            using (writer.WriteBlock("steps:"))
            {
#pragma warning disable CA1308
                var parameters = Parameters
                   .Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ parameters.{z.Name} }}}}'")
                   .ToArray()
                   .JoinSpace();
#pragma warning restore CA1308

                foreach (var step in Steps)
                {
                    step.Write(writer, parameters);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;
#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.AzurePipelines
{
    public class AzurePipelinesStep
    {
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string ScriptPath { get; set; } = null!;
        public IEnumerable<string> InvokedTargets { get; set; } = Array.Empty<string>();

        public void Write(CustomFileWriter writer, string parameters)
        {
            using (writer.WriteBlock($"- pwsh: ./{ScriptPath} {InvokedTargets.JoinSpace()} --skip {parameters}".TrimEnd()))
            {
                writer.WriteLine($"displayName: {DisplayName.SingleQuote()}");
            }
        }
    }
}
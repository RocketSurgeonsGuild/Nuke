using Nuke.Common.CI.AzurePipelines.Configuration;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.AzurePipelines
{
    public class AzurePipelinesStep
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ScriptPath { get; set; }
        public string[] InvokedTargets { get; set; }

        public void Write(CustomFileWriter writer, string parameters)
        {
            using (writer.WriteBlock($"- pwsh: ./{ScriptPath} {InvokedTargets.JoinSpace()} --skip {parameters}".TrimEnd()))
            {
                writer.WriteLine($"displayName: Nuke {DisplayName.SingleQuote()}");
            }
        }
    }
}
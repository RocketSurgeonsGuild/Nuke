using System;
using System.Linq;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.CI.AzurePipelines;
using JetBrains.Annotations;
using System.Diagnostics;

namespace Rocket.Surgery.Nuke.AzurePipelines.Configuration
{
    public class AzurePipelinesStage : AzurePipelinesConfigurationEntity
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public AzurePipelinesImageName? Image { get; set; }
        public AzurePipelinesStage[] Dependencies { get; set; }
        public AzurePipelinesJob[] Jobs { get; set; }

        public override void Write(CustomFileWriter writer)
        {
            using (writer.WriteBlock($"- stage: {Name}"))
            {
                writer.WriteLine($"displayName: {DisplayName.SingleQuote()}");
                writer.WriteLine($"dependsOn: [ {Dependencies.Select(x => x.Name).JoinComma()} ]");

                if (Image != null)
                {
                    using (writer.WriteBlock("pool:"))
                    {
                        writer.WriteLine($"vmImage: {Image?.Name.SingleQuote()}");
                    }
                }

                using (writer.WriteBlock("jobs:"))
                {
                    Jobs.ForEach(x => x.Write(writer));
                }
            }
        }
    }

    /// <summary>
    /// See <a href="https://docs.microsoft.com/en-us/azure/devops/pipelines/agents/hosted">Microsoft-hosted agents</a>
    /// </summary>
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public class AzurePipelinesImageName
    {
        public const string WindowsLatest = "windows-latest";
        public const string Windows2019 = "windows-2019";
        public const string Vs2017Win2016 = "vs2017-win2016";
        public const string Vs2015Win2012R2 = "vs2015-win2012r2";
        public const string Win1803 = "win1803";
        public const string Ubuntu1604 = "ubuntu-16.04";
        public const string Ubuntu1804 = "ubuntu-18.04";
        public const string UbuntuLatest = "ubuntu-latest";
        public const string MacOsLatest = "macOS-latest";
        public const string MacOs1014 = "macOS-10.14";
        public const string MacOs1013 = "macOS-10.13";
        public AzurePipelinesImageName(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }
        public static implicit operator string(AzurePipelinesImageName image)
        {
            return image.Name;
        }
        public static implicit operator AzurePipelinesImageName(string name)
        {
            return new AzurePipelinesImageName(name);
        }
    }
}

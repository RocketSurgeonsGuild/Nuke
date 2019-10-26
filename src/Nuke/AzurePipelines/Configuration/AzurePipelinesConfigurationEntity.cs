using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.AzurePipelines.Configuration
{
    public abstract class AzurePipelinesConfigurationEntity
    {
        public abstract void Write(CustomFileWriter writer);
    }
}
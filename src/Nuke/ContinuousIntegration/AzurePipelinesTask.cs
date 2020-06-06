using System;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public class AzurePipelinesTask
    {
        public AzurePipelinesTask()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
    }
}
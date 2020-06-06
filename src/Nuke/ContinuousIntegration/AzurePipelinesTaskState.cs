using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public class AzurePipelinesTaskState : Enumeration
    {
        public static readonly AzurePipelinesTaskState Unknown = new AzurePipelinesTaskState
            { Value = nameof(Unknown) };

        public static readonly AzurePipelinesTaskState Initialized = new AzurePipelinesTaskState
            { Value = nameof(Initialized) };

        public static readonly AzurePipelinesTaskState InProgress = new AzurePipelinesTaskState
            { Value = nameof(InProgress) };

        public static readonly AzurePipelinesTaskState Completed = new AzurePipelinesTaskState
            { Value = nameof(Completed) };
    }
}
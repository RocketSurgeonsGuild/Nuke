using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public class AzurePipelinesTaskResult : Enumeration
    {
        public static readonly AzurePipelinesTaskResult Succeeded = new AzurePipelinesTaskResult
            { Value = nameof(Succeeded) };

        public static readonly AzurePipelinesTaskResult SucceededWithIssues = new AzurePipelinesTaskResult
            { Value = nameof(SucceededWithIssues) };

        public static readonly AzurePipelinesTaskResult Failed = new AzurePipelinesTaskResult
            { Value = nameof(Failed) };

        public static implicit operator string(AzurePipelinesTaskResult configuration) => configuration.Value;
    }
}
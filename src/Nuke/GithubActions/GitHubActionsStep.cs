using Nuke.Common.CI;

namespace Rocket.Surgery.Nuke.GithubActions
{
    public abstract class GitHubActionsStep : ConfigurationEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Readme
{
    internal class GithubReleaseSection : IBadgeSection
    {
        public string Name => "Github Release";

        public string ConfigKey => "github";

        public string Process(
            IDictionary<object, object> config,
            IMarkdownReferences references,
            ICanUpdateReadme build
        )
        {
            var url = references.AddReference(
                "github-release",
                $"https://github.com/{config["owner"]}/{config["repository"]}/releases/latest"
            );
            var badge = references.AddReference(
                "github-release-badge",
                $"https://img.shields.io/github/release/{config["owner"]}/{config["repository"]}.svg?logo=github&style=flat",
                "Latest Release"
            );
            return $"[!{badge}]{url}";
        }
    }
}
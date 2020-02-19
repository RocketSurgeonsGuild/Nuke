using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Readme
{
    internal class CodecovSection : IBadgeSection
    {
        public string Name => "Codecov";

        public string ConfigKey => "github";

        public string Process(
            IDictionary<object, object> config,
            IMarkdownReferences references,
            IRocketBoosterBuild build
        )
        {
            var url = references.AddReference(
                "codecov",
                $"https://codecov.io/gh/{config["owner"]}/{config["repository"]}"
            );
            var badge = references.AddReference(
                "codecov-badge",
                $"https://img.shields.io/codecov/c/github/{config["owner"]}/{config["repository"]}.svg?color=E03997&label=codecov&logo=codecov&logoColor=E03997&style=flat",
                "Code Coverage"
            );
            return $"[!{badge}]{url}";
        }
    }
}
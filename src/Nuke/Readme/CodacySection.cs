using System.Collections.Generic;
using System.Dynamic;

namespace Rocket.Surgery.Nuke.Readme
{
    class CodacySection : IBadgeSection
    {
        public string Name => "Codacy";

        public string ConfigKey => string.Empty;

        public string Process(IDictionary<object, object> config, IMarkdownReferences references, RocketBoosterBuild build)
        {
            if (!(config.TryGetValue("github", out var githubObj) && config.TryGetValue("codacy", out var codacyObj)))
            {
                return string.Empty;
            }

            var github = (IDictionary<object, object>)githubObj;
            var codacy = (IDictionary<object, object>)codacyObj;
            var url = references.AddReference("codacy", $"https://www.codacy.com/app/{github["owner"]}/{github["repository"]}");
            var badge = references.AddReference("codacy-badge", $"https://api.codacy.com/project/badge/Grade/{codacy["project"]}", "Codacy");
            return $"[!{badge}]{url}";
        }
    }
}

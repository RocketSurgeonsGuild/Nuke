using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Readme
{
    internal class AppVeyorHistory : IHistorySection
    {
        public string Name { get; } = "AppVeyor";

        public string ConfigKey { get; } = "appveyor";

        public (string badge, string history) Process(
            IDictionary<object, object> config,
            IMarkdownReferences references,
            ICanUpdateReadme build
        )
        {
            var url = references.AddReference(
                "appveyor",
                $"https://ci.appveyor.com/project/{config["account"]}/{config["build"]}"
            );
            var badge = references.AddReference(
                "appveyor-badge",
                $"https://img.shields.io/appveyor/ci/{config["account"]}/{config["build"]}.svg?color=00b3e0&label=appveyor&logo=appveyor&logoColor=00b3e0&style=flat",
                "AppVeyor Status"
            );
            var historyUrl = references.AddReference(
                "appveyor-history",
                $"https://ci.appveyor.com/project/{config["account"]}/{config["build"]}/history"
            );
            var historyBadge = references.AddReference(
                "appveyor-history-badge",
                $"https://buildstats.info/appveyor/chart/{config["account"]}/{config["build"]}?includeBuildsFromPullRequest=false",
                "AppVeyor History"
            );

            return ( $"[!{badge}]{url}", $"[!{historyBadge}]{historyUrl}" );
        }
    }
}
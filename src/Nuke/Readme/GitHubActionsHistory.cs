namespace Rocket.Surgery.Nuke.Readme;

internal class GitHubActionsHistory : IHistorySection
{
    public string Name { get; } = "GitHub Actions";

    public string ConfigKey { get; } = "github";

    public (string badge, string history) Process(
        IDictionary<object, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        var url = references.AddReference(
            "github",
            $"https://github.com/{config["owner"]}/{config["repository"]}/actions?query=workflow%3Aci"
        );
        var badge = references.AddReference(
            "github-badge",
            $"https://img.shields.io/github/workflow/status/{config["owner"]}/{config["repository"]}/ci.svg?label=github&logo=github&color=b845fc&logoColor=b845fc&style=flat",
            "GitHub Actions Status"
        );
        var historyBadge = references.AddReference(
            "github-history-badge",
            $"https://buildstats.info/github/chart/{config["owner"]}/{config["repository"]}?includeBuildsFromPullRequest=false",
            "GitHub Actions History"
        );

        return ( $"[!{badge}]{url}", $"[!{historyBadge}]{url}" );
    }
}

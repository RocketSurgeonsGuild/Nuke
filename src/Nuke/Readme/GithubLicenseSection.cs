namespace Rocket.Surgery.Nuke.Readme;

internal class GithubLicenseSection : IBadgeSection
{
    public string Process(
        IDictionary<object, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        var url = references.AddReference(
            "github-license",
            $"https://github.com/{config["owner"]}/{config["repository"]}/blob/master/LICENSE"
        );
        var badge = references.AddReference(
            "github-license-badge",
            $"https://img.shields.io/github/license/{config["owner"]}/{config["repository"]}.svg?style=flat",
            "License"
        );
        return $"[!{badge}]{url}";
    }

    public string ConfigKey => "github";
    public string Name => "Github Release";
}

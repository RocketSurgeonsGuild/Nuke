namespace Rocket.Surgery.Nuke.Readme;

internal class CodecovSection : IBadgeSection
{
    public string Process(
        IDictionary<object, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
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

    public string Name => "Codecov";

    public string ConfigKey => "github";
}

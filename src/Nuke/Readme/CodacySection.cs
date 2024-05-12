namespace Rocket.Surgery.Nuke.Readme;

internal class CodacySection : IBadgeSection
{
    public string Name => "Codacy";

    public string ConfigKey => string.Empty;

    public string Process(
        IDictionary<object, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        if (!( config.TryGetValue("github", out var githubObj) && config.TryGetValue("codacy", out var codacyObj) )) return string.Empty;

        // ReSharper disable once NullableWarningSuppressionIsUsed
        var github = (IDictionary<object, object>)githubObj!;
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var codacy = (IDictionary<object, object>)codacyObj!;
        var url = references.AddReference(
            "codacy",
            $"https://www.codacy.com/app/{github["owner"]}/{github["repository"]}"
        );
        var badge = references.AddReference(
            "codacy-badge",
            $"https://api.codacy.com/project/badge/Grade/{codacy["project"]}",
            "Codacy"
        );
        return $"[!{badge}]{url}";
    }
}
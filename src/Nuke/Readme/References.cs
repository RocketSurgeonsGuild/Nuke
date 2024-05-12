using System.Text;

namespace Rocket.Surgery.Nuke.Readme;

/// <summary>
///     This section is used to allow for clean markdown references so that the readme file isn't complete cluttered with image
///     urls and links.
/// </summary>
public class References : IMarkdownReferences, IReadmeSection
{
    private readonly Dictionary<string, string> _references = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string AddReference(string name, string value, string? altText = null)
    {
        var key = $"[{name}]";
        if (string.IsNullOrEmpty(altText))
            altText = "";
        else
            altText = $" \"{altText}\"";

        _references.Add(key, $"{value}{altText}");
        return key;
    }

    /// <inheritdoc />
    public string Name { get; } = "generated references";

    /// <inheritdoc />
    public string ConfigKey { get; } = string.Empty;

    /// <inheritdoc />
    public string Process(
        IDictionary<string, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        var sb = new StringBuilder();
        foreach (var item in _references)
        {
            sb.Append(item.Key).Append(": ").AppendLine(item.Value);
        }

        return sb.ToString();
    }
}

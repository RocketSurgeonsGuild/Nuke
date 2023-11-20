using System.Collections.ObjectModel;

namespace Rocket.Surgery.Nuke.Readme;

/// <summary>
///     A general sections container, used to replace sections in the markdown
/// </summary>
[PublicAPI]
public class Sections
{
    private readonly Dictionary<string, IReadmeSection> _sections = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Gets a list of all the sections for markdown use
    /// </summary>
    internal IReadOnlyDictionary<string, IReadmeSection> AllSections => new ReadOnlyDictionary<string, IReadmeSection>(_sections);

    /// <summary>
    ///     Adds a new section.
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public Sections Add(IReadmeSection section)
    {
        _sections.Add(section.Name, section);
        return this;
    }
}

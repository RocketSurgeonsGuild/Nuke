using System.Dynamic;
using System.Text.RegularExpressions;

using YamlDotNet.Serialization;

namespace Rocket.Surgery.Nuke.Readme;

/// <summary>
///     The generic class used to contain all the sections, badges, histories and references.
/// </summary>
[PublicAPI]
public partial class ReadmeUpdater
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public ReadmeUpdater()
    {
        Sections = new();
        Badges = new();
        History = new();
        References = new();
        Sections
           .Add(Badges)
           .Add(History)
           .Add(References)
            ;
        Sections.Add(new NugetPackagesSection());
        History
           .Add(new AzurePipelinesHistory())
           .Add(new GitHubActionsHistory())
           .Add(new AppVeyorHistory())
            ;
        Badges
           .Add(new GithubReleaseSection())
           .Add(new GithubLicenseSection())
           .Add(new CodecovSection())
           .Add(new CodacySection())
            ;
    }

    /// <summary>
    ///     <para>Updates the given markdown content with all the sections replaced.</para>
    ///     <para>
    ///         The "generated references" is special and will always be run through last, to make sure all sections can
    ///         contribute references.
    ///     </para>
    /// </summary>
    /// <param name="content"></param>
    /// <param name="build"></param>
    /// <returns></returns>
    public async Task<string> Process(string content, IHaveSolution build)
    {
        var nukeDataRegex = MyRegex();
        var match = nukeDataRegex.Match(content);
        var yaml = string.Join(Environment.NewLine, match.Groups.Cast<Group>().Skip(1).Select(x => x.Value));
        var d = new DeserializerBuilder()
           // .WithNamingConvention(new CamelCaseNamingConvention())
           .Build();
        using var reader = new StringReader(yaml.Trim('\n', '\r'));
        var config = d.Deserialize<ExpandoObject>(reader);

        var sectionRegex = MyRegex1();

        var sections = sectionRegex.Matches(content);

        var ranges = new List<(int start, int length, string content)>();
        foreach (var sectionMatch in sections
                                    .GroupBy(x => x.Groups[1].Value)
                                    .OrderByDescending(x => x.Key != "generated references")
                )
        {
            var sectionName = sectionMatch.First().Groups[1].Value;
            if (!Sections.AllSections.TryGetValue(sectionName, out var section))
                throw new NotImplementedException("Section " + sectionName + " is not supported!");

            var sectionStart = sectionMatch.First().Captures[0];
            var sectionEnd = sectionMatch.Last().Captures[0];
            var newSectionContent = await section.Process(config, References, build);
            ranges.Add(
                (sectionStart.Index + sectionStart.Length,
                  sectionEnd.Index - ( sectionStart.Index + sectionStart.Length ), newSectionContent)
            );
        }

        foreach (var range in ranges.OrderByDescending(x => x.start))
        {
            content = string.Concat(
                content.AsSpan(0, range.start),
                Environment.NewLine,
                range.content,
                content.AsSpan(range.start + range.length)
            );
        }

        return content;
    }

    /// <summary>
    ///     The badges container
    /// </summary>
    public Badges Badges { get; }

    /// <summary>
    ///     The history container
    /// </summary>
    public Histories History { get; set; }

    /// <summary>
    ///     The references container for markdown references
    /// </summary>
    public References References { get; }

    /// <summary>
    ///     The sections container
    /// </summary>
    public Sections Sections { get; }

    [GeneratedRegex("<!-- nuke-data(.*?)-->", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline, "en-US")]
    private static partial Regex MyRegex();

    [GeneratedRegex("<!-- (.*?) -->", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex1();
}

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// The generic class used to contain all the sections, badges, histories and references.
    /// </summary>
    [PublicAPI]
    public class ReadmeUpdater
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ReadmeUpdater()
        {
            Sections = new Sections();
            Badges = new Badges();
            History = new Histories();
            References = new References();
            Sections
               .Add(Badges)
               .Add(History)
               .Add(References)
                ;
            Sections.Add(new NugetPackagesSection());
            History
               .Add(new AzurePipelinesHistory())
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
        /// The sections container
        /// </summary>
        public Sections Sections { get; }

        /// <summary>
        /// The badges container
        /// </summary>
        public Badges Badges { get; }

        /// <summary>
        /// The history container
        /// </summary>
        public Histories History { get; set; }

        /// <summary>
        /// The references container for markdown references
        /// </summary>
        public References References { get; }

        /// <summary>
        /// <para>Updates the given markdown content with all the sections replaced.</para>
        /// <para>
        /// The "generated references" is special and will always be run through last, to make sure all sections can
        /// contribute references.
        /// </para>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        public string Process(string content, IRocketBoosterBuild build)
        {
            var nukeDataRegex = new Regex(
                "<!-- nuke-data(.*?)-->",
                RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase
            );
            var match = nukeDataRegex.Match(content);
            var yaml = string.Join("\n", match.Groups.Cast<Group>().Skip(1).Select(x => x.Value));
            var d = new DeserializerBuilder()
                // .WithNamingConvention(new CamelCaseNamingConvention())
               .Build();
            using var reader = new StringReader(yaml.Trim('\n'));
            var config = d.Deserialize<ExpandoObject>(reader);

            var sectionRegex = new Regex(
                "<!-- (.*?) -->",
                RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase
            );

            var sections = sectionRegex.Matches(content);

            var ranges = new List<(int start, int length, string content)>();
            foreach (var sectionMatch in sections.OfType<Match>()
               .GroupBy(x => x.Groups[1].Value)
               .OrderByDescending(x => x.Key != "generated references")
            )
            {
                var sectionName = sectionMatch.First().Groups[1].Value;
                if (!Sections.AllSections.TryGetValue(sectionName, out var section))
                {
                    throw new NotImplementedException("Section " + sectionName + " is not supported!");
                }

                var sectionStart = sectionMatch.First().Captures[0];
                var sectionEnd = sectionMatch.Last().Captures[0];
                var newSectionContent = section.Process(config, References, build);
                ranges.Add(
                    ( sectionStart.Index + sectionStart.Length,
                      sectionEnd.Index - ( sectionStart.Index + sectionStart.Length ), newSectionContent )
                );
            }

            foreach (var range in ranges.OrderByDescending(x => x.start))
            {
                content = content.Substring(0, range.start)
                  + "\n" + range.content + content.Substring(range.start + range.length);
            }

            return content;
        }
    }
}
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// A container for build histories that you want to show on the readme
    /// </summary>
    [PublicAPI]
    public class Histories : IReadmeSection
    {
        private readonly List<IHistorySection> _sections = new List<IHistorySection>();

        /// <summary>
        /// Adds a new history section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Histories Add(IHistorySection section)
        {
            _sections.Add(section);
            return this;
        }

        /// <inheritdoc />
        public string Name => "history badges";

        /// <inheritdoc />
        public string ConfigKey => string.Empty;

        /// <inheritdoc />
        public string Process(IDictionary<string, object> config, IMarkdownReferences references, RocketBoosterBuild build)
        {
            var results = new List<(string name, string badge, string history)>();
            foreach (var section in _sections)
            {
                var subConfig = string.IsNullOrEmpty(section.ConfigKey) ? config.ToDictionary(x => (object)x.Key, x => x.Value) : config.TryGetValue(section.ConfigKey, out var o) ? o as IDictionary<object, object> : null;
                // Assume if not configured, it will never be able to be rendered
                if (subConfig is null) continue;
                var (badge, history) = section.Process(subConfig, references, build);
                results.Add((section.Name, badge, history));
            }
            var sb = new StringBuilder();
            sb.AppendLine($"| {string.Join(" | ", results.Select(z => z.name))} |");
            sb.AppendLine($"| {string.Join(" | ", results.Select(z => string.Join("", Enumerable.Range(0, z.name.Length).Select(x => "-"))))} |");
            sb.AppendLine($"| {string.Join(" | ", results.Select(z => z.badge))} |");
            sb.AppendLine($"| {string.Join(" | ", results.Select(z => z.history))} |");
            return sb.ToString();
        }
    }
}

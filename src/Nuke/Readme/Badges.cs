using System.Collections.Generic;
using System.Text;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// A container for badges that you want to show on the readme
    /// </summary>
    public class Badges : IReadmeSection
    {
        private readonly List<IBadgeSection> _sections = new List<IBadgeSection>();

        /// <summary>
        /// Adds a new Badge section
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Badges Add(IBadgeSection section)
        {
            _sections.Add(section);
            return this;
        }

        /// <inheritdoc />
        public string Name => "badges";

        /// <inheritdoc />
        public string ConfigKey => string.Empty;

        /// <inheritdoc />
        public string Process(
            IDictionary<string, object> config,
            IMarkdownReferences references,
            IRocketBoosterBuild build
        )
        {
            var sb = new StringBuilder();
            foreach (var section in _sections)
            {
                var subConfig = string.IsNullOrEmpty(section.ConfigKey) ?
                    config.ToDictionary(x => (object)x.Key, x => x.Value) :
                    config.TryGetValue(section.ConfigKey, out var o) ? o as IDictionary<object, object> : null;
                // Assume if not configured, it will never be able to be rendered
                if (subConfig is null)
                {
                    continue;
                }

                var result = section.Process(subConfig, references, build);
                if (string.IsNullOrWhiteSpace(result))
                {
                    continue;
                }

                sb.AppendLine(result);
            }

            return sb.ToString();
        }
    }
}
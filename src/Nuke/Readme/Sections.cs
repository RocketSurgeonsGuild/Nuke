using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// A general sections container, used to replace sections in the markdown
    /// </summary>
    [PublicAPI]
    public class Sections
    {
        private readonly IDictionary<string, IReadmeSection> _sections = new Dictionary<string, IReadmeSection>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a new section.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Sections Add(IReadmeSection section)
        {
            _sections.Add(section.Name, section);
            return this;
        }

        /// <summary>
        /// Gets a list of all the sections for markdown use
        /// </summary>
        internal IReadOnlyDictionary<string, IReadmeSection> AllSections => new ReadOnlyDictionary<string, IReadmeSection>(_sections);
    }
}

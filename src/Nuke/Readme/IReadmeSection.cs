using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// Interface is used to add a custom section that will be replaced in the readme with markdown content
    /// </summary>
    public interface IReadmeSection
    {
        /// <summary>
        /// The name of the section
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The configuration key, if you expect to get configuration from the yaml block.
        /// </summary>
        string ConfigKey { get; }

        /// <summary>
        /// Returns the markdown that will produce the badge
        /// </summary>
        /// <param name="config"></param>
        /// <param name="references"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        string Process(IDictionary<string, object> config, IMarkdownReferences references, IReadmeUpdater build);
    }
}
using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// Interface is used to add another badge to the `badges` container in the readme.
    /// </summary>
    public interface IBadgeSection
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
        string Process(IDictionary<object, object> config, IMarkdownReferences references, ICanUpdateReadme build);
    }
}
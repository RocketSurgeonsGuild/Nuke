using System.Collections.Generic;
using System.Dynamic;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// Interface is used to add another badge to the `history badges` container in the readme.
    /// </summary>
    public interface IHistorySection
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
        (string badge, string history) Process(IDictionary<object, object> config, IMarkdownReferences references, RocketBoosterBuild build);
    }
}

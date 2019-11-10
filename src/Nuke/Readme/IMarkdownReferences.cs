namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// A container interface for markdown references
    /// `[somekey]: somevalue "some alt text"`
    /// </summary>
    public interface IMarkdownReferences
    {
        /// <summary>
        /// Adds a reference with optional alt text
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="altText"></param>
        /// <returns></returns>
        string AddReference(string name, string value, string? altText = null);
    }
}

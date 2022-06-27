namespace Rocket.Surgery.Nuke;

/// <summary>
///     Allows defining custom solution updater patterns.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SolutionUpdaterConfigurationAttribute : Attribute
{
    /// <summary>
    ///     The files that will show up relative to the solution directory.
    /// </summary>
    public string[] AdditionalRelativeFolderFilePatterns { get; }

    /// <summary>
    ///     The files that will show up relative to the solution configuration directory.
    /// </summary>
    public string[] AdditionalConfigFolderFilePatterns { get; }

    /// <summary>
    ///     The default constructor.
    /// </summary>
    /// <param name="additionalRelativeFolderFilePatterns"></param>
    /// <param name="additionalConfigFolderFilePatterns"></param>
    public SolutionUpdaterConfigurationAttribute(string[]? additionalRelativeFolderFilePatterns = null, string[]? additionalConfigFolderFilePatterns = null)
    {
        AdditionalRelativeFolderFilePatterns = additionalRelativeFolderFilePatterns ?? Array.Empty<string>();
        AdditionalConfigFolderFilePatterns = additionalConfigFolderFilePatterns ?? Array.Empty<string>();
    }
}

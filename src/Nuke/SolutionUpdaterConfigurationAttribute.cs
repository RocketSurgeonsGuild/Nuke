namespace Rocket.Surgery.Nuke;

/// <summary>
///     Allows defining custom solution updater patterns.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SolutionUpdaterConfigurationAttribute : Attribute
{
    /// <summary>
    ///     The default constructor.
    /// </summary>
    /// <param name="additionalRelativeFolderFilePatterns"></param>
    /// <param name="additionalConfigFolderFilePatterns"></param>
    /// <param name="additionalIgnoreFolderFilePatterns"></param>
    public SolutionUpdaterConfigurationAttribute(
        string[]? additionalRelativeFolderFilePatterns = null,
        string[]? additionalConfigFolderFilePatterns = null,
        string[]? additionalIgnoreFolderFilePatterns = null
    )
    {
        AdditionalRelativeFolderFilePatterns = additionalRelativeFolderFilePatterns ?? [];
        AdditionalConfigFolderFilePatterns = additionalConfigFolderFilePatterns ?? [];
        AdditionalIgnoreFolderFilePatterns = additionalIgnoreFolderFilePatterns ?? [];
    }

    /// <summary>
    ///     The files that will show up relative to the solution directory.
    /// </summary>
    public string[] AdditionalRelativeFolderFilePatterns { get; }

    /// <summary>
    ///     The files that will show up relative to the solution configuration directory.
    /// </summary>
    public string[] AdditionalConfigFolderFilePatterns { get; }

    /// <summary>
    ///     The files or paths that will be ignored when updating the solution
    /// </summary>
    public string[] AdditionalIgnoreFolderFilePatterns { get; }
}

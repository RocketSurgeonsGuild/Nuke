namespace Rocket.Surgery.Nuke;

/// <summary>
///     Allows defining custom solution updater patterns.
/// </summary>
/// <remarks>
///     The default constructor.
/// </remarks>
/// <param name="additionalRelativeFolderFilePatterns"></param>
/// <param name="additionalConfigFolderFilePatterns"></param>
/// <param name="additionalIgnoreFolderFilePatterns"></param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SolutionUpdaterConfigurationAttribute(
    string[]? additionalRelativeFolderFilePatterns = null,
    string[]? additionalConfigFolderFilePatterns = null,
    string[]? additionalIgnoreFolderFilePatterns = null
    ) : Attribute
{
    /// <summary>
    ///     The files that will show up relative to the solution directory.
    /// </summary>
    public string[] AdditionalRelativeFolderFilePatterns { get; } = additionalRelativeFolderFilePatterns ?? [];

    /// <summary>
    ///     The files that will show up relative to the solution configuration directory.
    /// </summary>
    public string[] AdditionalConfigFolderFilePatterns { get; } = additionalConfigFolderFilePatterns ?? [];

    /// <summary>
    ///     The files or paths that will be ignored when updating the solution
    /// </summary>
    public string[] AdditionalIgnoreFolderFilePatterns { get; } = additionalIgnoreFolderFilePatterns ?? [];
}

namespace Rocket.Surgery.Nuke;

/// <summary>
///     The settings for the nuke build
/// </summary>
public static class Settings
{
    /// <summary>
    ///     The default github job name, so that it can be overridden
    /// </summary>
    public static string DefaultGithubJobName { get; set; } = "build";
}

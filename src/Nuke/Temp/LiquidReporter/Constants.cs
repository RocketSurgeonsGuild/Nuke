namespace Rocket.Surgery.Nuke.Temp.LiquidReporter;

/// <summary>
///     Constants for Cli tool.
/// </summary>
internal static class Constants
{
    /// <summary>
    ///     The file extension of MD file.
    /// </summary>
    public const string MdFileExtension = ".md";

    /// <summary>
    ///     Key for custom titles.
    /// </summary>
    public const string TitleKey = "Title";

    /// <summary>
    ///     Default titles when parameter not provided.
    /// </summary>
    public const string DefaultTitle = "Test Run";
}

/// <summary>
///     Application exit codes
/// </summary>
internal enum ExitCodes
{
    Success = 0,
    InvalidCommandLine = 1,
    ReportGenerationError = 2,
    ReportSaveError = 3
}

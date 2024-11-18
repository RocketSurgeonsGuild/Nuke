namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines an step that runs code in the given shell
/// </summary>
[PublicAPI]
public class RunStep : BaseGitHubActionsStep
{
    private static readonly string[] separator = { "\r\n", "\n" };

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public RunStep(string name) : base(name) { }

    // ReSharper disable once NullableWarningSuppressionIsUsed
    /// <summary>
    ///     The script to run
    /// </summary>
    public string Run { get; set; } = null!;

    /// <summary>
    ///     The shell to run with
    /// </summary>
    public GithubActionShell? Shell { get; set; }

    /// <summary>
    ///     The working directory where the script is run
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);
        using (writer.Indent())
        {
            if (!string.IsNullOrWhiteSpace(WorkingDirectory))
                writer.WriteLine($"working-directory: {WorkingDirectory}");
            if (!string.IsNullOrWhiteSpace(Shell?.ToString()))
                writer.WriteLine($"shell: {Shell}");
            writer.WriteLine("run: |");
            using (writer.Indent())
            {
                foreach (var line in Run.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    writer.WriteLine(line.Trim());
                }
            }
        }
    }
}

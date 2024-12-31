#pragma warning disable CA2227
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Define a job with github actions
/// </summary>
/// <remarks>
///     The default constructor
/// </remarks>
/// <param name="name"></param>
/// <exception cref="ArgumentNullException"></exception>
[PublicAPI]
public class RocketSurgeonsGithubWorkflowJob(string name) : RocketSurgeonsGithubActionsJobBase(name)
{
    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            writer.WriteLine($"uses: {Uses}");
            writer.WriteKeyValues("with", With);
            writer.WriteKeyValues("secrets", Secrets);
        }
    }

    /// <summary>
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> Secrets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The action to use.
    /// </summary>
    public string? Uses { get; set; }

    /// <summary>
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> With { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

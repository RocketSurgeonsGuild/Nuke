using Nuke.Common.CI;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Base job used for generation github actions yaml
/// </summary>
public abstract class RocketSurgeonsGithubActionsJobBase : ConfigurationEntity
{
    /// <summary>
    ///     Create the base job
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected RocketSurgeonsGithubActionsJobBase(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        Name = name;
    }

    /// <summary>
    ///     The name of the job
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The dependencies of this job
    /// </summary>
    public Dictionary<string, string> Outputs { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The dependencies of this job
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The dependencies of this job
    /// </summary>
    public List<string> Needs { get; set; } = new();

    /// <summary>
    ///     The condition to run this job under
    /// </summary>
    public GithubActionCondition? If { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"{Name}:");
        using (writer.Indent())
        {
            if (Needs.Any())
            {
                writer.WriteLine("needs:");
                using (writer.Indent())
                {
                    foreach (var need in Needs)
                    {
                        writer.WriteLine($"- {need}");
                    }
                }
            }

            writer.WriteKeyValues("outputs", Outputs);
            writer.WriteKeyValues("env", Environment);

            if (!string.IsNullOrWhiteSpace(If?.ToString()))
            {
                writer.WriteLine($"if: {If}");
            }
        }
    }
}

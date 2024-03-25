using Nuke.Common.CI;

#pragma warning disable CA2227 // Collection properties should be read only
#pragma warning disable CA1002 // Do not expose generic lists
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
    public string Name { get; set; }

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

    /// <summary>
    ///     The concurrency of the job
    /// </summary>
    public RocketSurgeonsGithubActionsConcurrency? Concurrency { get; set; }

    /// <summary>
    ///     The defaults of the job
    /// </summary>
    public RocketSurgeonsGithubActionsDefaults? Defaults { get; set; }

    /// <summary>
    ///     The outputs of this job
    /// </summary>
    public List<GitHubActionsStepOutput> Outputs { get; set; } = new();

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"{Name}:");
        using (writer.Indent())
        {
            if (Needs.Count > 0)
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

            writer.WriteKeyValues("outputs", Outputs.ToDictionary(z => $"{z.StepName}{z.OutputName.Pascalize()}".Camelize(), z => z.ToString()));
            writer.WriteKeyValues("env", Environment);
            if (Concurrency is { } concurrency)
            {
                writer.WriteLine("concurrency:");
                concurrency.Write(writer);
            }

            if (Defaults is { } defaults)
            {
                writer.WriteLine("defaults:");
                defaults.Write(writer);
            }

            if (!string.IsNullOrWhiteSpace(If?.ToString()))
            {
                writer.WriteLine($"if: {If}");
            }
        }
    }
}
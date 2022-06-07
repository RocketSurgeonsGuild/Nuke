using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions.Configuration;

#pragma warning disable CA1002
#pragma warning disable CA2227
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
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> Secrets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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
            writer.WriteKeyValues("secrets", Secrets);

            if (!string.IsNullOrWhiteSpace(If?.ToString()))
            {
                writer.WriteLine($"if: {If}");
            }
        }
    }
}

/// <summary>
///     Define a job with github actions
/// </summary>
[PublicAPI]
public class RocketSurgeonsGithubActionsJob : RocketSurgeonsGithubActionsJobBase
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public RocketSurgeonsGithubActionsJob(string name) : base(name)
    {
    }

    /// <summary>
    ///     The images to run on in a matrix
    /// </summary>
    public IEnumerable<string> Matrix { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    ///     The images to run on
    /// </summary>
    public IEnumerable<string> RunsOn { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    ///     The steps to run
    /// </summary>
    public List<GitHubActionsStep> Steps { get; set; } = new();

    /// <summary>
    ///     Should the job matrix fail fast, or wait for all to fail
    /// </summary>
    public bool FailFast { get; set; } = true;

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            if (Matrix.Count() > 1 || !FailFast)
            {
                writer.WriteLine("strategy:");
            }

            using (writer.Indent())
            {
                if (!FailFast)
                {
                    writer.WriteLine("fail-fast: false");
                }

                if (Matrix.Count() > 1)
                {
                    writer.WriteLine("matrix:");

                    using (writer.Indent())
                    {
                        writer.WriteLine($"os: [{string.Join(", ", Matrix)}]");
                    }
                }
            }

            if (!Matrix.Any() && RunsOn.Any())
            {
                writer.WriteLine($"runs-on: [{string.Join(", ", RunsOn)}]");
            }
            else if (Matrix.Count() == 1)
            {
                writer.WriteLine($"runs-on: {Matrix.First()}");
            }
            else if (Matrix.Count() > 1)
            {
                writer.WriteLine("runs-on: ${{ matrix.os }}");
            }

            writer.WriteLine("steps:");
            using (writer.Indent())
            {
                foreach (var step in Steps)
                {
                    step.Write(writer);
                }
            }
        }
    }
}

/// <summary>
///     Define a job with github actions
/// </summary>
[PublicAPI]
public class RocketSurgeonsGithubWorkflowJob : RocketSurgeonsGithubActionsJobBase
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public RocketSurgeonsGithubWorkflowJob(string name) : base(name)
    {
    }

    /// <summary>
    ///     The action to use.
    /// </summary>
    public string? Uses { get; set; }

    /// <summary>
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> With { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            writer.WriteLine($"uses: {Uses}");
            writer.WriteKeyValues("with", With);
        }
    }
}

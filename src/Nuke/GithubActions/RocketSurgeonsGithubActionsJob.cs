using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;

#pragma warning disable CA1002
#pragma warning disable CA2227
namespace Rocket.Surgery.Nuke.GithubActions;

public abstract class RocketSurgeonsGithubActionsJobBase : ConfigurationEntity
{
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
    /// The dependencies of this job
    /// </summary>
    public Dictionary<string, string> Outputs { get; set; } = new();

    /// <summary>
    /// The dependencies of this job
    /// </summary>
    public List<string> Needs { get; set; } = new();

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

            if (Outputs.Any())
            {
                writer.WriteLine("outputs:");
                using (writer.Indent())
                {
                    Outputs.ForEach(x => writer.WriteLine($"{x.Key}: '{x.Value}'"));
                }
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
    ///     The images to use (for docker)
    /// </summary>
    public IEnumerable<GitHubActionsImage> Images { get; set; } = Enumerable.Empty<GitHubActionsImage>();

    /// <summary>
    ///     The condition to run this job under
    /// </summary>
    public GithubActionCondition? If { get; set; }

    /// <summary>
    ///     The steps to run
    /// </summary>
    public List<GitHubActionsStep> Steps { get; set; } = new();

    public bool FailFast { get; set; } = true;

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            if (Images.Count() > 1 || !FailFast)
            {
                writer.WriteLine("strategy:");
            }

            using (writer.Indent())
            {
                if (!FailFast)
                {
                    writer.WriteLine("fail-fast: false");
                }

                if (Images.Count() > 1)
                {
                    writer.WriteLine("matrix:");

                    using (writer.Indent())
                    {
                        var images = string.Join(
                            ", ", Images.Select(image => image.GetValue().Replace(".", "_", StringComparison.Ordinal))
                        );
                        writer.WriteLine($"os: [{images}]");
                    }
                }
            }

            if (Images.Count() == 1)
            {
                writer.WriteLine($"runs-on: {Images.First().GetValue().Replace(".", "_", StringComparison.Ordinal)}");
            }
            else if (Images.Count() > 1)
            {
                writer.WriteLine("runs-on: ${{ matrix.os }}");
            }

            if (!string.IsNullOrWhiteSpace(If?.ToString()))
            {
                writer.WriteLine($"if: {If}");
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

    /// <summary>
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> Secrets { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            writer.WriteLine($"uses: {Uses}");

            if (With.Any())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    With.ForEach(x => writer.WriteLine($"{x.Key}: '{x.Value}'"));
                }
            }

            if (Secrets.Any())
            {
                writer.WriteLine("secrets:");
                using (writer.Indent())
                {
                    Secrets.ForEach(x => writer.WriteLine($"{x.Key}: '{x.Value}'"));
                }
            }
        }
    }
}

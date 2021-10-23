using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Tooling;

#pragma warning disable CA1002
#pragma warning disable CA2227
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Define a job with github actions
/// </summary>
[PublicAPI]
public class RocketSurgeonsGithubActionsJob : ConfigurationEntity
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public RocketSurgeonsGithubActionsJob(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        Name = name;
    }

    /// <summary>
    ///     The name of the job
    /// </summary>
    public string Name { get; }

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

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"{Name}:");

        using (writer.Indent())
        {
            writer.WriteLine("strategy:");
            using (writer.Indent())
            {
                writer.WriteLine("fail-fast: false");
                writer.WriteLine("matrix:");

                using (writer.Indent())
                {
                    var images = string.Join(
                        ", ", Images.Select(image => image.GetValue().Replace(".", "_", StringComparison.Ordinal))
                    );
                    writer.WriteLine($"os: [{images}]");
                }
            }

            writer.WriteLine("runs-on: ${{ matrix.os }}");
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

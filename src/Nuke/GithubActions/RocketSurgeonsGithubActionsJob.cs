using Nuke.Common.CI.GitHubActions.Configuration;

#pragma warning disable CA1002, CA2227
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
public class RocketSurgeonsGithubActionsJob(string name) : RocketSurgeonsGithubActionsJobBase(name)
{
    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            Permissions?.Write(writer);

            if (Matrix.Count() > 1 || !FailFast) writer.WriteLine("strategy:");

            using (writer.Indent())
            {
                if (!FailFast) writer.WriteLine("fail-fast: false");

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
                writer.WriteLine($"runs-on: [{string.Join(", ", RunsOn)}]");
            else if (Matrix.Count() == 1)
                writer.WriteLine($"runs-on: {Matrix.First()}");
            else if (Matrix.Count() > 1) writer.WriteLine("runs-on: ${{ matrix.os }}");

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

    /// <summary>
    ///     Should the job matrix fail fast, or wait for all to fail
    /// </summary>
    public bool FailFast { get; set; } = true;

    /// <summary>
    ///     The images to run on in a matrix
    /// </summary>
    public IEnumerable<string> Matrix { get; set; } = [];

    /// <summary>
    ///     The permissions of this workflow
    /// </summary>
    public GitHubActionsPermissions? Permissions { get; set; }

    /// <summary>
    ///     The images to run on
    /// </summary>
    public IEnumerable<string> RunsOn { get; set; } = [];

    /// <summary>
    ///     The steps to run
    /// </summary>
    public List<GitHubActionsStep> Steps { get; set; } = [];

    internal IDictionary<object, object> InternalData { get; } = new Dictionary<object, object>();
}

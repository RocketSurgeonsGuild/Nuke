using Nuke.Common.CI.GitHubActions.Configuration;

// ReSharper disable MemberCanBePrivate.Global
#pragma warning disable CA2227
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A customized base action step
/// </summary>
public abstract class BaseGitHubActionsStep : GitHubActionsStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected BaseGitHubActionsStep(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        StepName = name;
    }

    /// <summary>
    ///     The step id to use
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     The step name
    /// </summary>
    public string StepName { get; }

    /// <summary>
    ///     The condition
    /// </summary>
    public GithubActionCondition? If { get; set; }

    /// <summary>
    ///     Should this continue on error or not
    /// </summary>
    public bool ContinueOnError { get; set; }

    /// <summary>
    ///     The dependencies of this job
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     The outputs of this step
    /// </summary>
#pragma warning disable CA1002
    public List<GitHubActionsOutput> Outputs { get; set; } = new();
#pragma warning restore CA1002

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine($"- name: {ComputeStepName(StepName)}");
        using (writer.Indent())
        {
            if (!string.IsNullOrWhiteSpace(Id)) writer.WriteLine($"id: {Id}");

            writer.WriteKeyValues("env", Environment);

            if (!string.IsNullOrWhiteSpace(If?.ToString())) writer.WriteLine($"if: {If}");

            if (ContinueOnError) writer.WriteLine("continue-on-error: true");
        }
    }

    /// <summary>
    ///     Configure the step name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual string ComputeStepName(string name)
    {
        return Symbols.StepName(name);
    }
}

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     An output from a workflow job in github actions
/// </summary>
/// <param name="JobName"></param>
/// <param name="OutputName"></param>
/// <param name="Description"></param>
public record GitHubActionsWorkflowOutput(string JobName, string OutputName, string? Description = null)
{
    /// <summary>
    ///     Convert to the github actions template for the workflow output
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $$$"""${{ jobs.{{{JobName}}}.outputs.{{{OutputName}}} }}""";
    }
}

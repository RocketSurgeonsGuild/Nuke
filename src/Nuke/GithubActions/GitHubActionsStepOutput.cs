namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines an a step output variable
/// </summary>
/// <param name="StepName"></param>
/// <param name="OutputName"></param>
/// <param name="Description"></param>
public record GitHubActionsStepOutput(string StepName, string OutputName, string? Description = null)
{
    /// <summary>
    ///     Convert to the github actions template for this output variable
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $$$"""${{ steps.{{{StepName}}}.outputs.{{{OutputName}}} }}""";
    }

    /// <summary>
    ///     Convert to a workflow output variable
    /// </summary>
    /// <param name="jobName"></param>
    /// <returns></returns>
    public GitHubActionsWorkflowOutput ToWorkflow(string jobName)
    {
        return new(jobName, $"{StepName}{OutputName.Pascalize()}".Camelize(), Description);
    }
}

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines a github actions output variable
/// </summary>
/// <param name="OutputName"></param>
/// <param name="Description"></param>
public record GitHubActionsOutput(string OutputName, string? Description = null)
{
    /// <summary>
    ///     Convert the output to a step output
    /// </summary>
    /// <param name="stepName"></param>
    /// <returns></returns>
    public GitHubActionsStepOutput ToStep(string stepName) => new(stepName, OutputName, Description);
}

namespace Rocket.Surgery.Nuke.GithubActions;

public record GitHubActionsWorkflowOutput(string JobName, string OutputName, string? Description = null)
{
    public override string ToString()
    {
        return $$$"""${{ jobs.{{{JobName}}}.outputs.{{{OutputName}}} }}""";
    }
}

public record GitHubActionsStepOutput(string StepName, string OutputName, string? Description = null)
{
    public override string ToString()
    {
        return $$$"""${{ steps.{{{StepName}}}.outputs.{{{OutputName}}} }}""";
    }

    public GitHubActionsWorkflowOutput ToWorkflow(string jobName)
    {
        return new(jobName, $"{StepName}{OutputName.Pascalize()}".Camelize(), Description);
    }
}

public record GitHubActionsOutput(string OutputName, string? Description = null)
{
    public GitHubActionsStepOutput ToStep(string stepName)
    {
        return new(stepName, OutputName, Description);
    }
}

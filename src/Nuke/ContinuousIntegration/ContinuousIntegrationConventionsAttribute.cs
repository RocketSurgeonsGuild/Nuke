using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Enhances the build with some github actions specific conventions
/// </summary>
[PublicAPI]
#pragma warning disable CA1813
[AttributeUsage(AttributeTargets.Class)]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ContinuousIntegrationConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
#pragma warning restore CA1813
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    /// <inheritdoc />
    public void OnBuildFinished()
    {
        if (Build is not IHaveArtifacts nukeBuild)
        {
            return;
        }

        switch (nukeBuild.Host)
        {
            case GitHubActions:
                {
                    EmitTestSummaryMarkdown(Build, GitHubActions.Instance.StepSummaryFile);
                    break;
                }

            default:
                {
                    EmitTestSummaryMarkdown(Build, nukeBuild.ArtifactsDirectory / "github" / "summary.md");
                    break;
                }
        }
    }

    private void EmitTestSummaryMarkdown(INukeBuild build, AbsolutePath summary)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.ExecutionPlan.Any(z => z.Name == nameof(IHaveTestTarget.Test))
         && build is IHaveTestArtifacts testResultReports
         && testResultReports.TestResultsDirectory.GlobFiles("**/*.trx") is
         { Count: > 0, } results)
        {
            //            summary.TouchFile();
            //            var reporter = new LiquidReporter(results.Select(z => z.ToString()), Log.Logger);
            //            var report = reporter.Run("Test results");
            //            summary.WriteAllText(summary.ReadAllText().TrimStart() + "\n" + report);
            _ = DotNetTasks.DotNet(
                new Arguments()
                   .Add("liquid")
                   .Add("--inputs {0}", results.Select(z => $"File={z}"), ' ', quoteMultiple: true)
                   .Add("--output-file {0}", summary)
                   .RenderForExecution(),
                workingDirectory: build.RootDirectory,
                logOutput: false
            );
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.ExecutionPlan.Any(z => z.Name == nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary))
         && build is IGenerateCodeCoverageSummary codeCoverage
         && ( codeCoverage.CoverageSummaryDirectory / "Summary.md" ).FileExists())
        {
            _ = summary.TouchFile();
            var coverageSummary = ( codeCoverage.CoverageSummaryDirectory / "Summary.md" ).ReadAllText();
            if (coverageSummary.IndexOf("|**Name**", StringComparison.Ordinal) is > -1 and var index)
            {
                coverageSummary = coverageSummary[..( index - 1 )];
            }

            _ = summary.WriteAllText(coverageSummary + summary.ReadAllText().TrimStart());
        }
    }
}

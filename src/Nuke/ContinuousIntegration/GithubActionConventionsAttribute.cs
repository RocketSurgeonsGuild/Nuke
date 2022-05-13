using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.Temp.LiquidReporter;
using Serilog;

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Enhances the build with some github actions specific conventions
/// </summary>
[PublicAPI]
public sealed class GithubActionConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnBuildFinished(NukeBuild build)
    {
        if (build is not INukeBuild nukeBuild) return;
        if (nukeBuild.Host != GitHubActions.Instance) return;
        if (EnvironmentInfo.GetVariable<AbsolutePath>("GITHUB_STEP_SUMMARY") is not { } summary) return;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.ExecutionPlan.Any(z => z.Name == nameof(IHaveTestTarget.Test))
         && build is IHaveTestArtifacts testResultReports
         && testResultReports.TestResultsDirectory.GlobFiles("**/*.trx") is
                { Count: > 0 } results)
        {
            FileSystemTasks.Touch(summary);
            var reporter = new LiquidReporter(results.Select(z => z.ToString()), Log.Logger);
            var report = reporter.Run("Test results");
            TextTasks.WriteAllText(summary, TextTasks.ReadAllText(summary).TrimStart() + "\n" + report);
//            DotNet(
//                new Arguments()
//                   .Add("liquid")
//                   .Add("--inputs {0}", results.Select(z => "File=" + z), ' ', quoteMultiple: true)
//                   .Add("--output {0}", EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY"))
//                   .ToString()
//            );
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.ExecutionPlan.Any(z => z.Name == nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary))
         && build is IGenerateCodeCoverageSummary codeCoverage
         && ( codeCoverage.CoverageSummaryDirectory / "Summary.md" ).Exists())
        {
            FileSystemTasks.Touch(summary);
            var coverageSummary = TextTasks.ReadAllText(codeCoverage.CoverageSummaryDirectory / "Summary.md");
            if (coverageSummary.IndexOf("|**Name**", StringComparison.Ordinal) is > -1 and var index)
            {
                var topCoverageSummary = coverageSummary[..( index - 1 )];
                var detailsCoverageSummary = coverageSummary[index..];
                coverageSummary = topCoverageSummary + "\n<details>\n<summary>Coverage Details</summary>\n" + detailsCoverageSummary + "\n</details>";
            }

            TextTasks.WriteAllText(summary, TextTasks.ReadAllText(summary).TrimStart() + "\n" + coverageSummary);
        }
    }
}

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
//        if (nukeBuild.Host != GitHubActions.Instance) return;
        if (EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY") is not { Length: > 0 } summary) return;

        // ReSharper disable once SuspiciousTypeConversion.Global

        if (build.InvokedTargets.Any(z => z.Name == nameof(IHaveTestTarget.Test)) && build is IHaveTestArtifacts testResultReports
                                                                                  && testResultReports.TestResultsDirectory.GlobFiles("**/*.trx") is
                                                                                         { Count: > 0 } results)
        {
            FileSystemTasks.Touch(summary);
            var reporter = new LiquidReporter(results.Select(z => z.ToString()), Log.Logger);
            var report = reporter.Run("Test results");
            var summaryContent = TextTasks.ReadAllText(summary);
            TextTasks.WriteAllText(summary, summaryContent + "\n" + report);
//            DotNet(
//                new Arguments()
//                   .Add("liquid")
//                   .Add("--inputs {0}", results.Select(z => "File=" + z), ' ', quoteMultiple: true)
//                   .Add("--output {0}", EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY"))
//                   .ToString()
//            );
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.InvokedTargets.Any(z => z.Name == nameof(IHaveTestTarget.Test)) && build is IGenerateCodeCoverageSummary codeCoverage
                                                                                  && ( codeCoverage.CoverageSummaryDirectory / "summary.md" ).Exists())
        {
            FileSystemTasks.Touch(summary);
            var coverageSummary = TextTasks.ReadAllText(codeCoverage.CoverageSummaryDirectory / "summary.md");
            var summaryContent = TextTasks.ReadAllText(summary);
            TextTasks.WriteAllText(summary, summaryContent + "\n" + coverageSummary);
        }
    }
}

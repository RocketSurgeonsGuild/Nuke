using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Rocket.Surgery.Nuke.Temp.LiquidReporter;
using Serilog;

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Enhances the build with some github actions specific conventions
/// </summary>
public sealed class GithubActionConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
{
    /// <inheritdoc />
    public void OnBuildFinished(NukeBuild build)
    {
        if (build is not INukeBuild nukeBuild) return;
        if (nukeBuild.Host != GitHubActions.Instance) return;
        if (EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY") is not { Length: > 0 } summary) return;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build is IGenerateCodeCoverageSummary codeCoverage && ( codeCoverage.CoverageSummaryDirectory / "summary.md" ).Exists())
        {
            FileSystemTasks.CopyFile(
                codeCoverage.CoverageSummaryDirectory / "summary.md",
                EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY")
            );
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build is IHaveTestArtifacts testResultReports && testResultReports.TestResultsDirectory.GlobFiles("**/*.trx") is { Count: > 0 } results)
        {
            var reporter = new LiquidReporter(results.Select(z => z.ToString()), Log.Logger);
            reporter.Run("Test results", EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY")!);
//            DotNet(
//                new Arguments()
//                   .Add("liquid")
//                   .Add("--inputs {0}", results.Select(z => "File=" + z), ' ', quoteMultiple: true)
//                   .Add("--output {0}", EnvironmentInfo.GetVariable<string>("GITHUB_STEP_SUMMARY"))
//                   .ToString()
//            );
        }
    }
}

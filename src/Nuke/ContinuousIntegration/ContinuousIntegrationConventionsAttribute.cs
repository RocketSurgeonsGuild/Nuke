using System.Diagnostics;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Rocket.Surgery.Nuke.ContinuousIntegration;

/// <summary>
///     Enhances the build with some github actions specific conventions
/// </summary>
[PublicAPI]
#pragma warning disable CA1813
[AttributeUsage(AttributeTargets.Class)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public partial class ContinuousIntegrationConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished
#pragma warning restore CA1813
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();


    private void EmitTestSummaryMarkdown(INukeBuild build, AbsolutePath summary)
    {
        var toolInstalled = DotNetTool.IsInstalled("liquidtestreports.cli");
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (build.ExecutionPlan.Any(z => z.Name == nameof(IHaveTestTarget.Test))
         && build is IHaveTestArtifacts { TestResultsDirectory: { } testResultsDirectory, }
         && testResultsDirectory.GlobFiles("**/*.trx") is { Count: > 0, } results)
        {
            var pathArgs =
                results
                   .Select(
                        path => testResultsDirectory.GetRelativePathTo(
                            path.RenameWithoutExtension(static path => path.Name.Replace("[", "").Replace("]", ""))
                        )
                    )
                   .Aggregate(new Arguments(), (arguments, path) => arguments.Add("--inputs {value}", $"File={path}"));

            //            summary.TouchFile();
            //            var reporter = new LiquidReporter(results.Select(z => z.ToString()), Log.Logger);
            //            var report = reporter.Run("Test results");
            //            summary.WriteAllText(summary.ReadAllText().TrimStart() + "\n" + report);
            _ = DotNetTasks.DotNet(
                new Arguments()
                   .Add("liquid")
                   .Concatenate(pathArgs)
                   .Add("--output-file {0}", summary)
                   .RenderForExecution(),
                testResultsDirectory,
                logOutput: true /* temp */,
                logInvocation: build.Verbosity == Verbosity.Verbose
            );
        }
        else if (!toolInstalled)
        {
            Log.Warning("liquidtestreports.cli is not installed, skipping test summary generation");
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
}
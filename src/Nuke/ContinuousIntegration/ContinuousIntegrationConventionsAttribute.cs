using System.Diagnostics;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;

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
//        // ReSharper disable once SuspiciousTypeConversion.Global
//        if (build.ExecutionPlan.Any(z => z.Name == nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary))
//         && build is IGenerateCodeCoverageSummary codeCoverage
//         && ( codeCoverage.CoverageSummaryDirectory / "Summary.md" ).FileExists())
//        {
//            _ = summary.TouchFile();
//            var coverageSummary = ( codeCoverage.CoverageSummaryDirectory / "Summary.md" ).ReadAllText();
//            if (coverageSummary.IndexOf("|**Name**", StringComparison.Ordinal) is > -1 and var index)
//            {
//                coverageSummary = coverageSummary[..( index - 1 )];
//            }
//
//            _ = summary.WriteAllText(coverageSummary + summary.ReadAllText().TrimStart());
//        }
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

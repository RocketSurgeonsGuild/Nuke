using System.Collections.Frozen;
using Buildalyzer;
using Serilog;

namespace Rocket.Surgery.Nuke.ProjectModel;

internal record ProjectAnalyzerResults(IProjectAnalyzer Project, FrozenSet<IAnalyzerResult> Results)
{
    public ProjectAnalyzerModel GetProjectForTargetFramework(string? targetFramework = null)
    {
        targetFramework ??= Results.Reverse().Select(z => z.TargetFramework).FirstOrDefault();
        return new(Results.FirstOrDefault(z => z.TargetFramework == targetFramework) ?? returnDefault(Results.Last(), Project.ProjectFile.Path));

        static ProjectAnalyzerModel returnDefault(IAnalyzerResult result, string contextPath)
        {
            Log.Warning(
                "No target framework specified or found for {ContextPath}, returning the last result.  Available target frameworks: {@TargetFrameworks}",
                contextPath,
                result.Analyzer.ProjectFile.TargetFrameworks
            );
            return new(result);
        }
    }
}
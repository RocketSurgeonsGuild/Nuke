using Nuke.Common.Execution;
using Nuke.Common.IO;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Attribute is used for conventions that will only run on a local build
///     Used an extension point to ensure the local build environment is configured correctly.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LocalBuildConventionsAttribute : BuildExtensionAttributeBase, IOnBuildFinished, IOnBuildInitialized, IOnBuildCreated
{
    private static void EnsureSolutionIsUptoDate(INukeBuild nukeBuild, IHaveSolution haveSolution)
    {
        var path = nukeBuild.TemporaryDirectory / "solution-last-updated-at";
        if (!path.FileExists())
        {
            using var _ = File.Create(path);
            _.Close();
        }
        else if (File.GetLastWriteTime(path) + TimeSpan.FromHours(1) > DateTime.Now)
        {
            return;
        }

        var attributes = nukeBuild
                        .GetType()
                        .GetCustomAttributes(true)
                        .OfType<SolutionUpdaterConfigurationAttribute>()
                        .ToArray();
        SolutionUpdater.UpdateSolution(
            haveSolution.Solution,
            attributes.SelectMany(z => z.AdditionalRelativeFolderFilePatterns),
            attributes.SelectMany(z => z.AdditionalConfigFolderFilePatterns)
        );
        File.SetLastWriteTime(path, DateTime.Now);
    }

    /// <inheritdoc />
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (!EnvironmentInfo.HasVariable("RSG_NUKE_LINT_STAGED")) return;

        Log.Logger = new LoggerConfiguration()
                    .ConfigureEnricher()
                    .ConfigureConsole(null)
                    .ConfigureLevel()
                    .CreateLogger();
    }

    /// <inheritdoc />
    public void OnBuildFinished()
    {
        if (Build is not ({ } nukeBuild and IHaveSolution)) return;
        if (nukeBuild.IsServerBuild) return;

        //        EnsureSolutionIsUptoDate(nukeBuild, haveSolution);
    }

    /// <inheritdoc />
    public override float Priority { get; set; } = -1000;

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        if (Build is not (INukeBuild nukeBuild and IHaveSolution haveSolution)) return;
        if (nukeBuild.IsServerBuild) return;

        EnsureSolutionIsUptoDate(nukeBuild, haveSolution);
    }
}

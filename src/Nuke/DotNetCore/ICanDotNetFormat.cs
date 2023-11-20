using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines the targets and properties for using DotNet Format
/// </summary>
public interface ICanDotNetFormat : IHaveSolution, ICanLint
{
    /// <summary>
    ///     The default severity to use for DotNetFormat
    /// </summary>
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.warn;

    /// <summary>
    ///     The dotnet format target
    /// </summary>
    public Target DotNetFormat => d =>
        d
           .TriggeredBy(PostLint)
           .After(Lint)
           .OnlyWhenStatic(() => IsLocalBuild)
           .Executes(
                () => LintPaths.Any()
                    ? DotNetTasks.DotNet($"format --severity {DotNetFormatSeverity} --include {string.Join(",", LintPaths)}")
                    : DotNetTasks.DotNet($"format --severity {DotNetFormatSeverity}")
            );

    /// <summary>
    ///     Use the jetbrains code cleanup tool to format the code if installed
    /// </summary>
    public Target JetBrainsCleanupCode => d =>
        d
           .TriggeredBy(Lint)
           .Before(DotNetFormat)
           .OnlyWhenStatic(() => !LintPaths.Any())
           .OnlyWhenStatic(() => DotNetTool.IsInstalled("jb"))
           .Executes(
                () => DotNetTasks.DotNet(
                    $""""jb cleanupcode "{Solution.Path}" --profile="Full Cleanup" --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal" """",
                    RootDirectory,
                    logOutput: true
                )
            );
}

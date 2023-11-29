using Nuke.Common.IO;
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
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.info;

    /// <summary>
    ///     The default profile to use for JetBrainsCleanupCode
    /// </summary>
    public string JetBrainsCleanupCodeProfile => "Full Cleanup";

    /// <summary>
    ///     The dotnet format target
    /// </summary>
    public Target DotNetFormat => d =>
                                      d
                                         .TriggeredBy(PostLint)
                                         .After(Lint)
                                         .OnlyWhenStatic(() => IsLocalBuild || LintPaths.Any())
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
                                                 .TriggeredBy(PostLint)
                                                 .After(DotNetFormat)
                                                 .OnlyWhenStatic(() => IsLocalBuild || LintPaths.Any())
                                                 .OnlyWhenStatic(() => DotNetTool.IsInstalled("jb"))
                                                 .Executes(
                                                      () => LintPaths.Any()
                                                          ? DotNetTasks.DotNet(
                                                              $""""jb cleanupcode "{Solution.Path}" --profile={JetBrainsCleanupCodeProfile} --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal" --include="{string.Join(";", LintPaths.Select(z => RootDirectory.GetRelativePathTo(z)))}" """",
                                                              RootDirectory,
                                                              logOutput: true
                                                          )
                                                          : DotNetTasks.DotNet(
                                                              $""""jb cleanupcode "{Solution.Path}" --profile={JetBrainsCleanupCodeProfile} --disable-settings-layers="GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal" """",
                                                              RootDirectory,
                                                              logOutput: true
                                                          )
                                                  );
    // --include
}

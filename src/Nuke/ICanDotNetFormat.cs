using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Defines the targets and properties for using DotNet Format
/// </summary>
public interface ICanDotNetFormat : IHaveSolution, ICanLint
{
    /// <summary>
    /// The default severity to use for DotNetFormat
    /// </summary>
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.warn;

    /// <summary>
    /// The dotnet format target
    /// </summary>
    public Target DotNetFormat => d =>
        d
           .DependentFor(Lint)
           .Executes(
                () => LintFiles.Count > 0
                    ? DotNetTasks.DotNet($"format --severity {DotNetFormatSeverity} --include {string.Join(",", LintFiles)}")
                    : DotNetTasks.DotNet($"format --severity {DotNetFormatSeverity}")
            );
}

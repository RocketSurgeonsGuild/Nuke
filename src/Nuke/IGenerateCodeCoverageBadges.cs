using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Generates a code coverage badges
/// </summary>
public interface IGenerateCodeCoverageBadges : ITriggerCodeCoverageReports, IGenerate
{
    /// <summary>
    ///     The directory where the badges will be places
    /// </summary>
    public AbsolutePath CoverageBadgeDirectory => CoverageDirectory / "badges";

    [Obsolete("Legacy target has been renamed to GenerateCodeCoverageBadges")]
    // ReSharper disable once InconsistentNaming
#pragma warning disable CS1591, CA1707
    public Target Generate_Code_Coverage_Badges =>
        d => d.DependsOn(GenerateCodeCoverageBadges).Unlisted();
#pragma warning restore CS1591, CA1707

    /// <summary>
    ///     Generate a code coverage badges for the given reports
    /// </summary>
    public Target GenerateCodeCoverageBadges => d => d
                                                    .After(GenerateCodeCoverageReportCobertura)
                                                    .TriggeredBy(TriggerCodeCoverageReports)
                                                    .Unlisted()
                                                    .OnlyWhenDynamic(() => InputReports.Any())
                                                    .Executes(
                                                         () => ReportGeneratorTasks.ReportGenerator(
                                                             s => WithTag(s)
                                                                 // .SetToolPath(toolPath)
                                                                 .SetFramework(Constants.ReportGeneratorFramework)
                                                                 .SetReports(InputReports)
                                                                 .SetTargetDirectory(CoverageBadgeDirectory)
                                                                 .SetReportTypes(ReportTypes.Badges)
                                                         )
                                                     );
}

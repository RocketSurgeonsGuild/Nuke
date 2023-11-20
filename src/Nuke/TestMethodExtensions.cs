using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.DotNetCore;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Methods used to allow common test tasks to be shared
/// </summary>
public static class TestMethodExtensions
{
    // ReSharper disable once CommentTypo
    /// <summary>
    /// Clean up any code coverage files from <see cref="IHaveCodeCoverage.CoverageDirectory"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="testResultsDirectory"></param>
    /// <param name="coverageDirectory"></param>
    /// <returns></returns>
    public static ITargetDefinition CollectCoverage(this ITargetDefinition target, AbsolutePath testResultsDirectory, AbsolutePath coverageDirectory)
    {
        return target.Executes(
            () =>
            {
                // Ensure anything that has been dropped in the test results from a collector is
                // into the coverage directory
                foreach (var file in testResultsDirectory
                                    .GlobFiles("**/*.cobertura.xml")
                                    .Where(x => Guid.TryParse(Path.GetFileName(x.Parent), out var _))
                                    .SelectMany(coverage => coverage.Parent.GlobFiles("*.*")))
                {
                    var folderName = Path.GetFileName(file.Parent);
                    var extensionPart = string.Join(".", Path.GetFileName(file).Split('.').Skip(1));
                    FileSystemTasks.CopyFile(
                        file,
                        coverageDirectory / $"{folderName}.{extensionPart}",
                        FileExistsPolicy.OverwriteIfNewer
                    );
                }
            }
        );
    }

    // ReSharper disable once CommentTypo
    /// <summary>
    /// Clean up any code coverage files from <see cref="IHaveCodeCoverage.CoverageDirectory"/>.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="coverageDirectory"></param>
    /// <returns></returns>
    public static ITargetDefinition CleanCoverageDirectory(this ITargetDefinition target, AbsolutePath coverageDirectory)
    {
        return target.Executes(
            () =>
            {
                coverageDirectory.GlobFiles("*.cobertura.xml", "*.opencover.xml", "*.json", "*.info")
                                 .Where(x => Guid.TryParse(Path.GetFileName(x).Split('.')[0], out var _))
                                 .ForEach(AbsolutePathExtensions.DeleteFile);
            }
        );
    }

    // ReSharper disable once CommentTypo
    /// <summary>
    /// A method that ensures the given runsettings file exists or creates a default one
    /// </summary>
    /// <param name="target"></param>
    /// <param name="runsettings"></param>
    /// <returns></returns>
    // ReSharper disable once IdentifierTypo
    // ReSharper disable once StringLiteralTypo
    public static ITargetDefinition EnsureRunSettingsExists(this ITargetDefinition target, AbsolutePath runsettings)
    {
        return target.Executes(
            async () =>
            {
                // ReSharper disable once IdentifierTypo
                // ReSharper disable once StringLiteralTypo
                if (!runsettings.FileExists())
                {
                    // ReSharper disable once StringLiteralTypo
                    runsettings = NukeBuild.TemporaryDirectory / "default.runsettings";
                    await using var tempFile = File.Open(runsettings, runsettings.FileExists() ? FileMode.Truncate : FileMode.CreateNew);
                    await typeof(ICanTestWithDotNetCore)
                       .Assembly
                       // ReSharper disable once NullableWarningSuppressionIsUsed
                       .GetManifestResourceStream("Rocket.Surgery.Nuke.default.runsettings")!.CopyToAsync(tempFile);
                }
            }
        );
    }
}

using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.DotNetCore;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Methods used to allow common test tasks to be shared
/// </summary>
public static class TestMethodExtensions
{
    // ReSharper disable once CommentTypo
    /// <summary>
    ///     A method that ensures the given runsettings file exists or creates a default one
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

using Nuke.Common.Tools.GitVersion;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     <see cref="GitVersion" /> extensions.
/// </summary>
public static class GitVersionFunctions
{
    /// <summary>
    ///     Gets the assembly version from the <see cref="GitVersion" />.
    /// </summary>
    /// <param name="gitVersion">The git version.</param>
    /// <returns>The converted semantic version with no alpha characters.</returns>
    public static string AssemblyVersion(this GitVersion? gitVersion) => FullSemanticVersion(gitVersion).RemoveAlphaCharacters();

    /// <summary>
    ///     Gets the full semantic version from <see cref="GitVersion" />.
    /// </summary>
    /// <param name="gitVersion">The git version.</param>
    /// <returns>The converted semantic version.</returns>
    public static string FullSemanticVersion(this GitVersion? gitVersion) => gitVersion?.FullSemVer.Replace('+', '.') ?? "";

    /// <summary>
    ///     Gets the Major.Minor.Patch from <see cref="GitVersion" />.
    /// </summary>
    /// <param name="gitVersion">The git version.</param>
    /// <returns>The converter major minor patch version.</returns>
    public static string MajorMinorPatch(this GitVersion? gitVersion) => $"{gitVersion?.Major}.{gitVersion?.Minor}.{gitVersion?.Patch}";

    /// <summary>
    ///     Gets the package version from the <see cref="GitVersion" />.
    /// </summary>
    /// <param name="gitVersion">The git version.</param>
    /// <returns>The converted nuget package version with no alpha characters.</returns>
    public static string PackageVersion(this GitVersion? gitVersion) => gitVersion?.NuGetVersionV2?.RemoveAlphaCharacters() ?? "";
}

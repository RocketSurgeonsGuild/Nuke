using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.Readme;

// ReSharper disable once SuspiciousTypeConversion.Global
// ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable CA1019
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Ensures that the given git hooks are defined in the .git directory
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class EnsureReadmeIsUpdatedAttribute : BuildExtensionAttributeBase, IOnBuildInitialized
{
    /// <summary>
    ///     Ensure readme gets updated
    /// </summary>
    /// <param name="fileName"></param>
    public EnsureReadmeIsUpdatedAttribute(string fileName)
    {
        ReadmeFilePath = fileName;
    }

    /// <summary>
    ///     Ensure the readme gets updated
    /// </summary>
    public EnsureReadmeIsUpdatedAttribute()
    {
        ReadmeFilePath = "Readme.md";
    }

    /// <summary>
    ///     The path to the readme file
    /// </summary>
    public string ReadmeFilePath { get; set; }

    /// <inheritdoc />
    public void OnBuildInitialized(
        NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan
    )
    {
        if (!NukeBuild.IsLocalBuild || build is not IHaveSolution buildSolution) return;
        using var block = Logger.Block("Update Readme");

        var readmeContent = File.ReadAllText(NukeBuild.RootDirectory / ReadmeFilePath);
        readmeContent = new ReadmeUpdater().Process(readmeContent, buildSolution);
        File.WriteAllText(NukeBuild.RootDirectory / ReadmeFilePath, readmeContent);
    }
}

using Nuke.Common.IO;
using Nuke.Common.Tools.NuGet;

namespace Rocket.Surgery.Nuke.MsBuild;

/// <summary>
///     Pack using msbuild
/// </summary>
public interface ICanPackWithMsBuild : IHavePackTarget,
                                       IHaveBuildTarget,
                                       IHaveTestTarget,
                                       IHaveNuGetPackages,
                                       IHaveGitVersion,
                                       IHaveConfiguration,
                                       ICan
{
    /// <summary>
    ///     The directory where templates will be placed
    /// </summary>
    public static AbsolutePath NuspecDirectory => NukeBuild.RootDirectory / ".nuspec";

    /// <summary>
    ///     nuget pack
    /// </summary>
    public Target NetPack => _ => _
                                 .DependsOn(Build)
                                 .After(Test)
                                 .Executes(
                                      () =>
                                      {
                                          foreach (var project in NuspecDirectory.GlobFiles("*.nuspec"))
                                          {
                                              NuGetTasks
                                                 .NuGetPack(
                                                      settings =>
                                                          settings
                                                             .SetTargetPath(project)
                                                             .SetConfiguration(Configuration)
                                                             .SetGitVersionEnvironment(GitVersion)
                                                             .SetVersion(GitVersion?.NuGetVersionV2)
                                                             .SetOutputDirectory(NuGetPackageDirectory)
                                                             .EnableSymbols()
                                                  );
                                          }
                                      }
                                  );
}

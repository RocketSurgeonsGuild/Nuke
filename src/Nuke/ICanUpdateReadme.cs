using Nuke.Common.IO;
using Rocket.Surgery.Nuke.Readme;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     A tool to update the readme
/// </summary>
public interface ICanUpdateReadme : IHaveSolution
{
    /// <summary>
    ///     The readme updater that ensures that all the badges are in sync.
    /// </summary>
    [Readme]
    public ReadmeUpdater Readme => TryGetValue(() => Readme)!;

    /// <summary>
    ///     The path to the readme file
    /// </summary>
    public AbsolutePath ReadmeFilePath => NukeBuild.RootDirectory / "Readme.md";

    /// <summary>
    ///     Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while
    ///     keeping the rest of the readme correct.
    /// </summary>
    public Target GenerateReadme => d => d
                                        .Unlisted()
                                        .OnlyWhenStatic(() => IsLocalBuild)
                                        .Executes(
                                             () =>
                                             {
                                                 var readmeContent = File.ReadAllText(ReadmeFilePath);
                                                 readmeContent = Readme.Process(readmeContent, this);
                                                 File.WriteAllText(ReadmeFilePath, readmeContent);
                                             }
                                         );
}

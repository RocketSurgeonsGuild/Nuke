using System.IO;
using Nuke.Common;
using Nuke.Common.ValueInjection;
using Rocket.Surgery.Nuke.Readme;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// A tool to update the readme
    /// </summary>
    public interface ICanUpdateReadme : IHaveSolution, INukeBuild
    {
        /// <summary>
        /// The readme updater that ensures that all the badges are in sync.
        /// </summary>
        [Readme]
        public ReadmeUpdater Readme => ValueInjectionUtility.TryGetValue(() => Readme);

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while
        /// keeping the rest of the readme correct.
        /// </summary>
        public Target GenerateReadme => _ => _
           .Unlisted()
           .OnlyWhenStatic(() => NukeBuild.IsLocalBuild)
           .Executes(
                () =>
                {
                    var readmeContent = File.ReadAllText(NukeBuild.RootDirectory / "Readme.md");
                    readmeContent = Readme.Process(readmeContent, this);
                    File.WriteAllText(NukeBuild.RootDirectory / "Readme.md", readmeContent);
                }
            );
    }
}
using System;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Rocket.Surgery.Nuke.Readme;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Base build plan and tasks
    /// </summary>
    public interface IRocketBoosterBuild<T> : IRocketBoosterBuild
        where T : Configuration
    {
        /// <summary>
        /// Configuration to build - Default is 'Debug' (local) or 'Release' (server)
        /// </summary>
        T Configuration { get; }
    }

    public interface IRocketBoosterBuild
    {
        /// <summary>
        /// Force a clean build, otherwise leave some incremental build pieces
        /// </summary>
        bool Force { get; }

        /// <summary>
        /// The solution currently being build
        /// </summary>
        Solution Solution { get; }

        /// <summary>
        /// The Git Repository currently being built
        /// </summary>
        GitRepository? GitRepository { get; }

        /// <summary>
        /// The Git Version information either computed by GitVersion itself, or as defined by environment variables of the format
        /// `GITVERSION_*`
        /// </summary>
        GitVersion? GitVersion { get; }

        /// <summary>
        /// The readme updater that ensures that all the badges are in sync.
        /// </summary>
        ReadmeUpdater Readme { get; }

        /// <summary>
        /// The directory where samples will be placed
        /// </summary>
        AbsolutePath SampleDirectory { get; }

        /// <summary>
        /// The directory where sources will be placed
        /// </summary>
        AbsolutePath SourceDirectory { get; }

        /// <summary>
        /// The directory where templates will be placed
        /// </summary>
        AbsolutePath TemplatesDirectory { get; }

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        AbsolutePath TestDirectory { get; }

        /// <summary>
        /// The directory where tests will be placed
        /// </summary>
        AbsolutePath TestsDirectory { get; }

        /// <summary>
        /// The directory where artifacts are to be dropped
        /// </summary>
        AbsolutePath ArtifactsDirectory { get; }

        /// <summary>
        /// The directory where logs will be placed
        /// </summary>
        AbsolutePath LogsDirectory { get; }

        /// <summary>
        /// The directory where test results will be placed
        /// </summary>
        AbsolutePath TestResultsDirectory { get; }

        /// <summary>
        /// The directory where nuget packages will be placed
        /// </summary>
        AbsolutePath NuGetPackageDirectory { get; }

        /// <summary>
        /// The directory where publish output should be placed
        /// </summary>
        AbsolutePath PublishDirectory { get; }

        /// <summary>
        /// The directory where packaged output should be placed (zip, webdeploy, etc)
        /// </summary>
        AbsolutePath OutputDirectory { get; }

        /// <summary>
        /// The directory where coverage artifacts are to be dropped
        /// </summary>
        [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
        AbsolutePath CoverageDirectory { get; }

        /// <summary>
        /// prints the build information.
        /// </summary>
        Target BuildVersion { get; }

        /// <summary>
        /// clean all artifact directories
        /// </summary>
        Target Clean { get; }

        /// <summary>
        /// This will generate code coverage reports from emitted coverage data
        /// </summary>
        Target Generate_Code_Coverage_Reports { get; }

        /// <summary>
        /// Loops through the Readme to update sections that are automated to give nuget packages, build histories and more, while
        /// keeping the rest of the readme correct.
        /// </summary>
        Target GenerateReadme { get; }
    }
}
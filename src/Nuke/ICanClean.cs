using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines a target that cleans common directories
    /// </summary>
    public interface ICanClean : IHaveCleanTarget, IHaveBuildTarget
    {
        /// <summary>
        /// clean all artifact directories
        /// </summary>
        public new Target Clean => _ => _
           .Before(Build)
           .Executes(
                () =>
                {
                    if (this is IHaveArtifacts artifacts)
                    {
                        FileSystemTasks.EnsureCleanDirectory(artifacts.ArtifactsDirectory);
                        if (artifacts is IHaveOutputLogs logs)
                            FileSystemTasks.EnsureExistingDirectory(logs.LogsDirectory);
                        if (artifacts is IHaveTestArtifacts testArtifacts)
                            FileSystemTasks.EnsureExistingDirectory(testArtifacts.TestResultsDirectory);
                        if (artifacts is IHaveNuGetPackages nuGetArtifacts)
                            FileSystemTasks.EnsureExistingDirectory(nuGetArtifacts.NuGetPackageDirectory);
                        if (artifacts is IHavePublishArtifacts publishArtifacts)
                            FileSystemTasks.EnsureExistingDirectory(publishArtifacts.PublishDirectory);
                        if (artifacts is IHaveOutputArtifacts outputArtifacts)
                            FileSystemTasks.EnsureExistingDirectory(outputArtifacts.OutputArtifactsDirectory);
                    }

                    if (this is IHaveCodeCoverage codeCoverage)
                    {
                        FileSystemTasks.EnsureCleanDirectory(codeCoverage.CoverageDirectory);
                    }

                    // ReSharper disable SuspiciousTypeConversion.Global
                    if (this is IMayTheForceBeWithYou forceBeWithYou && forceBeWithYou.Force)
                    {
                        if (this is IComprehendSamples samples && FileSystemTasks.DirectoryExists(samples.SampleDirectory))
                        {
                            samples.SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IComprehendSources sources && FileSystemTasks.DirectoryExists(sources.SourceDirectory))
                        {
                            sources.SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IComprehendTemplates templates && FileSystemTasks.DirectoryExists(templates.TemplatesDirectory))
                        {
                            templates.TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }

                        if (this is IComprehendTests tests && FileSystemTasks.DirectoryExists(tests.TestsDirectory))
                        {
                            tests.TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                        }
                    }
                } // ReSharper restore SuspiciousTypeConversion.Global
            );
    }
}
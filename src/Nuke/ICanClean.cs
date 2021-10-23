using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a target that cleans common directories
/// </summary>
public interface ICanClean : IHaveCleanTarget, IHaveBuildTarget
{
    /// <summary>
    ///     clean all artifact directories
    /// </summary>
    public new Target Clean => _ => _
                                   .Before(Build)
                                   .Executes(
                                        () =>
                                        {
                                            if (this is IHaveArtifacts artifacts)
                                            {
                                                EnsureCleanDirectory(artifacts.ArtifactsDirectory);
                                                if (artifacts is IHaveOutputLogs logs)
                                                {
                                                    EnsureExistingDirectory(logs.LogsDirectory);
                                                }

                                                if (artifacts is IHaveTestArtifacts testArtifacts)
                                                {
                                                    EnsureExistingDirectory(testArtifacts.TestResultsDirectory);
                                                }

                                                if (artifacts is IHaveNuGetPackages nuGetArtifacts)
                                                {
                                                    EnsureExistingDirectory(nuGetArtifacts.NuGetPackageDirectory);
                                                }

                                                if (artifacts is IHavePublishArtifacts publishArtifacts)
                                                {
                                                    EnsureExistingDirectory(publishArtifacts.PublishDirectory);
                                                }

                                                if (artifacts is IHaveOutputArtifacts outputArtifacts)
                                                {
                                                    EnsureExistingDirectory(outputArtifacts.OutputArtifactsDirectory);
                                                }
                                            }

                                            if (this is IHaveCodeCoverage codeCoverage)
                                            {
                                                EnsureCleanDirectory(codeCoverage.CoverageDirectory);
                                            }

                                            // ReSharper disable SuspiciousTypeConversion.Global
                                            if (this is IMayTheForceBeWithYou forceBeWithYou && forceBeWithYou.Force)
                                            {
                                                if (this is IComprehendSamples samples && DirectoryExists(samples.SampleDirectory))
                                                {
                                                    samples.SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                                                }

                                                if (this is IComprehendSources sources && DirectoryExists(sources.SourceDirectory))
                                                {
                                                    sources.SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                                                }

                                                if (this is IComprehendTemplates templates && DirectoryExists(templates.TemplatesDirectory))
                                                {
                                                    templates.TemplatesDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                                                }

                                                if (this is IComprehendTests tests && DirectoryExists(tests.TestsDirectory))
                                                {
                                                    tests.TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                                                }
                                            }
                                        } // ReSharper restore SuspiciousTypeConversion.Global
                                    );
}

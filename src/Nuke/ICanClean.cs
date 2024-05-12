using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

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
    public new Target Clean => d => d
                                   .Before(Build)
                                   .Executes(
                                        () =>
                                        {
                                            if (this is IHaveArtifacts artifacts)
                                            {
                                                artifacts.ArtifactsDirectory.CreateOrCleanDirectory();
                                                if (artifacts is IHaveOutputLogs logs) logs.LogsDirectory.CreateDirectory();

                                                if (artifacts is IHaveTestArtifacts testArtifacts) testArtifacts.TestResultsDirectory.CreateDirectory();

                                                if (artifacts is IHaveNuGetPackages nuGetArtifacts) nuGetArtifacts.NuGetPackageDirectory.CreateDirectory();

                                                if (artifacts is IHavePublishArtifacts publishArtifacts) publishArtifacts.PublishDirectory.CreateDirectory();

                                                if (artifacts is IHaveOutputArtifacts outputArtifacts) outputArtifacts.OutputArtifactsDirectory.CreateDirectory();
                                            }

                                            if (this is IHaveCodeCoverage codeCoverage) codeCoverage.CoverageDirectory.CreateOrCleanDirectory();

                                            // ReSharper disable SuspiciousTypeConversion.Global
                                            if (this is IMayTheForceBeWithYou forceBeWithYou && forceBeWithYou.Force)
                                            {
                                                if (this is IComprehendSamples samples && samples.SampleDirectory.DirectoryExists()) samples.SampleDirectory.GlobDirectories("**/bin", "**/obj").ForEach(AbsolutePathExtensions.DeleteDirectory);

                                                if (this is IComprehendSources sources && sources.SourceDirectory.DirectoryExists()) sources.SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(AbsolutePathExtensions.DeleteDirectory);

                                                if (this is IComprehendTemplates templates && templates.TemplatesDirectory.DirectoryExists())
                                                    templates.TemplatesDirectory.GlobDirectories("**/bin", "**/obj")
                                                             .ForEach(AbsolutePathExtensions.DeleteDirectory);

                                                if (this is IComprehendTests tests && tests.TestsDirectory.DirectoryExists()) tests.TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(AbsolutePathExtensions.DeleteDirectory);
                                            }
                                        } // ReSharper restore SuspiciousTypeConversion.Global
                                    );
}

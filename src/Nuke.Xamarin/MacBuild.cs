using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.MSBuild;
using Rocket.Surgery.Nuke;

namespace Rocket.Surgery.Nuke.Xamarin
{
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    public class MacBuild : XamarinBuild
    {
        public Target XamarinMac => _ => _
            .DependsOn(Clean)
            .DependsOn(Restore)
            .DependsOn(Build);


        /// <summary>
        /// msbuild
        /// </summary>
        public override Target Build => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                MSBuildTasks
                    .MSBuild(settings =>
                        settings
                            .SetSolutionFile(Solution)
                            .SetConfiguration(Configuration)
                            .SetBinaryLogger(LogsDirectory / "build.binlog", LogImportType(IsLocalBuild))
                            .SetFileLogger(LogsDirectory / "build.log")
                            .SetGitVersionEnvironment(GitVersion)
                            .SetAssemblyVersion(GitVersion.AssemblySemVer)
                            .SetPackageVersion(GitVersion.NuGetVersionV2)
                            .SetTargets("Publish")
                            .SetOutDir(OutputDirectory));
            });

    }
}

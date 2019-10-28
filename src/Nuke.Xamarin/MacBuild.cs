using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Rocket.Surgery.Nuke;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin.Mac based applications
    /// </summary>
    public abstract class MacBuild : XamarinBuild
    {
        /// <summary>
        /// Core target that can be used to trigger all targets for this build
        /// </summary>
        public Target XamarinMac => _ => _
            .DependsOn(Clean)
            .DependsOn(Restore)
            .DependsOn(Build);

        /// <inheritdoc />
        public override Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() =>
            {
                NuGetTasks
                    .NuGetRestore(settings =>
                        settings
                            .SetSolutionDirectory(Solution)
                            .EnableNoCache());
            });


        /// <inheritdoc />
        public override Target Build => _ => _
            .DependsOn(Restore)
            .Executes(() =>
            {
                MSBuildTasks
                    .MSBuild(settings =>
                        settings
                            .SetSolutionFile(Solution)
                            .SetConfiguration(Configuration)
                            .SetBinaryLogger(LogsDirectory / "build.binlog")
                            .SetFileLogger(LogsDirectory / "build.log")
                            .SetGitVersionEnvironment(GitVersion)
                            .SetAssemblyVersion(GitVersion.AssemblySemVer)
                            .SetPackageVersion(GitVersion.NuGetVersionV2)
                            .SetTargets("Publish")
                            .SetOutDir(OutputDirectory));
            });

    }
}

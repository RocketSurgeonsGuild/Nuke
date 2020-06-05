using Nuke.Common;
using Nuke.Common.Tools.NuGet;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

#pragma warning disable 1591

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface ICanRestoreXamarin : IHaveGitVersion, IHaveSolution, IHaveCleanTarget, IHaveRestoreTarget
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        /// <remarks>https://developercommunity.visualstudio.com/content/problem/20550/cant-run-dotnet-restore.html</remarks>
        public new Target Restore => _ => _
           .DependsOn(Clean)
           .Executes(
                () => NuGetRestore(
                    settings =>
                        settings
                           .SetTargetPath(Solution)
                           .SetGitVersionEnvironment(GitVersion)
                           .SetNoCache(true)
                )
            );
    }
}
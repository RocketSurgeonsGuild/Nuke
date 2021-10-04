using Nuke.Common;
using Nuke.Common.Tooling;
using System.Collections.Generic;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface ICanFastlaneMatch : ICanFastlane, IHaveGitVersion, IHaveSolution, IHaveCleanTarget, IHaveRestoreTarget
    {
        /// <summary>
        /// nuget restore
        /// </summary>
        public new Target Match => _ => _
           .DependsOn(Clean)
           .Executes(() => ProcessTasks.StartProcess("fastlane", "", "", new Dictionary<string, string>()));
    }
}
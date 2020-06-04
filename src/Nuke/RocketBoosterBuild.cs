using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Base build plan and tasks
    /// </summary>
    [PublicAPI]
    [DotNetVerbosityMapping]
    [MSBuildVerbosityMapping]
    [NuGetVerbosityMapping]
    public abstract class RocketBoosterBuild : NukeBuild { }
}
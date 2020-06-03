using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.Readme;
using Temp.CleanupCode;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Temp.CleanupCode.CleanupCodeTasks;
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
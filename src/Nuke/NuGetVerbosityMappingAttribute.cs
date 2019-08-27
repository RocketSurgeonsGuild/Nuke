using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Mapping for nuget Verbosity
    /// </summary>
    public class NuGetVerbosityMappingAttribute : VerbosityMappingAttribute
    {
        /// <summary>
        /// Mapping for nuget Verbosity
        /// </summary>
        public NuGetVerbosityMappingAttribute()
                    : base(typeof(NuGetVerbosity))
        {
            Quiet = nameof(NuGetVerbosity.Quiet);
            Minimal = nameof(NuGetVerbosity.Normal);
            Normal = nameof(NuGetVerbosity.Normal);
            Verbose = nameof(NuGetVerbosity.Detailed);
        }
    }
}

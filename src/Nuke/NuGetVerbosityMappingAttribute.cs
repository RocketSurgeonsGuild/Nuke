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
    /// Mapping for nuget Verbosities
    /// </summary>
    public class NuGetVerbosityMappingAttribute : VerbosityMappingAttribute
    {
        /// <summary>
        /// Mapping for nuget Verbosities
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
    /// <summary>
    /// Mapping for nuget Verbosities
    /// </summary>
    public class DotNetCoreVerbosityMappingAttribute : VerbosityMappingAttribute
    {
        /// <summary>
        /// Mapping for nuget Verbosities
        /// </summary>
        public DotNetCoreVerbosityMappingAttribute()
                       : base(typeof(DotNetVerbosity))
        {
            Quiet = nameof(DotNetVerbosity.Quiet);
            Minimal = nameof(DotNetVerbosity.Minimal);
            Normal = nameof(DotNetVerbosity.Minimal);
            Verbose = nameof(DotNetVerbosity.Detailed);
        }
    }
}

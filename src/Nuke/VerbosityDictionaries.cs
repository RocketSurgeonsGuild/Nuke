using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Dictionaries for linking different verbosity enums together.
    /// </summary>
    public static class VerbosityDictionaries
    {
        /// <summary>
        /// NuGetVerbosityDictionary
        /// </summary>
        public static Dictionary<Verbosity, NuGetVerbosity> NuGetVerbosityDictionary =
            new Dictionary<Verbosity, NuGetVerbosity>
            {
                {Verbosity.Quiet, NuGetVerbosity.Quiet},
                {Verbosity.Minimal, NuGetVerbosity.Normal},
                {Verbosity.Normal, NuGetVerbosity.Normal},
                {Verbosity.Verbose, NuGetVerbosity.Detailed}
            };

        /// <summary>
        /// MSBuildVerbosityDictionary
        /// </summary>
        public static Dictionary<Verbosity, MSBuildVerbosity> MSBuildVerbosityDictionary =
            new Dictionary<Verbosity, MSBuildVerbosity>
            {
                {Verbosity.Quiet, MSBuildVerbosity.Quiet},
                {Verbosity.Minimal, MSBuildVerbosity.Minimal},
                {Verbosity.Normal, MSBuildVerbosity.Normal},
                {Verbosity.Verbose, MSBuildVerbosity.Diagnostic}
            };

        /// <summary>
        /// DotNetVerbosityDictionary
        /// </summary>
        public static Dictionary<Verbosity, DotNetVerbosity> DotNetVerbosityDictionary =
            new Dictionary<Verbosity, DotNetVerbosity>
            {
                {Verbosity.Quiet, DotNetVerbosity.Quiet},
                {Verbosity.Minimal, DotNetVerbosity.Minimal},
                {Verbosity.Normal, DotNetVerbosity.Normal},
                {Verbosity.Verbose, DotNetVerbosity.Diagnostic}
            };
    }
}

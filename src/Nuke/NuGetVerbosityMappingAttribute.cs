using Nuke.Common.Tooling;
using Nuke.Common.Tools.NuGet;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Mapping for nuget Verbosity
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class NuGetVerbosityMappingAttribute : VerbosityMappingAttribute
{
    /// <summary>
    ///     Mapping for nuget Verbosity
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

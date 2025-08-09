namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     A customized attribute to disable dotnet workload restore
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DisableDotnetWorkloadRestoreAttribute : Attribute
{
    /// <summary>
    /// Represents an attribute used to disable the restoration of .NET workloads.
    /// </summary>
    /// <remarks>
    /// This attribute, when applied to a class, sets the flag <c>Extensions.EnableDotNetWorkloadRestore</c>
    /// to <c>false</c> to disable the restoration process for workloads during the build.
    /// </remarks>
    public DisableDotnetWorkloadRestoreAttribute() => Extensions.EnableDotNetWorkloadRestore = false;
}

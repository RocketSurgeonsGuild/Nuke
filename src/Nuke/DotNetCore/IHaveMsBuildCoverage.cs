#pragma warning disable CA1033
namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines `CollectCoverage` as false to utilize the coverlet msbuild collector
/// </summary>
public interface IHaveMsBuildCoverage : IHaveCollectCoverage
{
    bool IHaveCollectCoverage.CollectCoverage => false;
}

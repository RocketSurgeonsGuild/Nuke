namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Defines `CollectCoverage` as flase to utilize the coverlet msbuild collector
    /// </summary>
    public interface IUseMsBuildCoverage : IHaveCollectCoverage
    {
        bool IHaveCollectCoverage.CollectCoverage => false;
    }
}
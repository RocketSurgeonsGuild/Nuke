namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Defines `CollectCoverage` as true to utilize the coverlet data collector for code coverage
    /// </summary>
    public interface IHaveDataCollector : IHaveCollectCoverage
    {
        bool IHaveCollectCoverage.CollectCoverage => true;
    }
}
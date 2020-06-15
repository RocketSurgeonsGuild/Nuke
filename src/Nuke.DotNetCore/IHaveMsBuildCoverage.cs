﻿namespace Rocket.Surgery.Nuke.DotNetCore
{
    /// <summary>
    /// Defines `CollectCoverage` as flase to utilize the coverlet msbuild collector
    /// </summary>
    public interface IHaveMsBuildCoverage : IHaveCollectCoverage
    {
        bool IHaveCollectCoverage.CollectCoverage => false;
    }
}
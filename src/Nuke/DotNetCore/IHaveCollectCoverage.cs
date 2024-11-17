namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines the `CollectCoverage` property
/// </summary>
public interface IHaveCollectCoverage : IHave
{
    /// <summary>
    ///     Determines if we use a coverage collector or some other coverage mechanism
    /// </summary>
    /// <value></value>
    bool CollectCoverage { get; }

    /// <summary>
    /// The coverage data collector
    /// </summary>
    public string DataCollector => "Code Coverage";
}

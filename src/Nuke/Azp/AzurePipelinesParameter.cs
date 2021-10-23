using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;

namespace Rocket.Surgery.Nuke.Azp;

/// <summary>
///     The azure pipelines parameter name
/// </summary>
public class AzurePipelinesParameter : ConfigurationEntity
{
    /// <summary>
    ///     The parameter name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The default name
    /// </summary>
    public string Default { get; set; } = null!;

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        using var a = writer.WriteBlock($"{Name}: '{Default}'");
    }
}

using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines.Configuration;

namespace Rocket.Surgery.Nuke.Azp;

/// <summary>
///     A collection of azure pipelines steps
/// </summary>
public class AzurePipelinesSteps : ConfigurationEntity
{
    /// <summary>
    ///     Write the given pipeline steps
    /// </summary>
    /// <param name="writer"></param>
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("#");
        if (Parameters.Count > 0)
            using (writer.WriteBlock("parameters:"))
            {
                foreach (var item in Parameters)
                {
                    item.Write(writer);
                }
            }

        using (writer.WriteBlock("steps:"))
        {
            #pragma warning disable CA1308
            var parameters = Parameters
                            .Select(z => $"--{z.Name.ToLowerInvariant()} '${{{{ parameters.{z.Name} }}}}'")
                            .ToArray()
                            .JoinSpace();
            #pragma warning restore CA1308

            foreach (var step in Steps)
            {
                step.Write(writer, parameters);
            }
        }
    }

    /// <summary>
    ///     The parameters for the pipeline step
    /// </summary>
    public IReadOnlyList<AzurePipelinesParameter> Parameters { get; set; } = Array.Empty<AzurePipelinesParameter>();

    /// <summary>
    ///     The steps to run with the given parameters
    /// </summary>
    public IReadOnlyList<AzurePipelinesStep> Steps { get; set; } = Array.Empty<AzurePipelinesStep>();
}

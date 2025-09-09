using Nuke.Common.CI.AzurePipelines.Configuration;

namespace Rocket.Surgery.Nuke.Azp;

// ReSharper disable NullableWarningSuppressionIsUsed
/// <summary>
///     An azure pipelines step
/// </summary>
public class AzurePipelinesStep
{
    /// <summary>
    ///     Write the pipelines step
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="parameters"></param>
    public void Write(CustomFileWriter writer, string parameters)
    {
        using (writer.WriteBlock(
                   $"- pwsh: {ScriptPath} {InvokedTargets.JoinSpace()} --skip {parameters}".TrimEnd()
               ))
        {
            writer.WriteLine($"displayName: {DisplayName.SingleQuote()}");
        }
    }

    /// <summary>
    ///     The step name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    ///     The display name
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    ///     The script path
    /// </summary>
    public string ScriptPath { get; set; } = null!;

    /// <summary>
    ///     The targets to invoke
    /// </summary>
    public IEnumerable<string> InvokedTargets { get; set; } = Array.Empty<string>();
}

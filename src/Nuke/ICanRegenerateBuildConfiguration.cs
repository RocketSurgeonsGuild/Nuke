using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Defines an interface that can regenerate all of the build configurations
/// </summary>
public interface ICanRegenerateBuildConfiguration
{
    private Target RegenerateBuildConfigurations => t =>
        t
           .Unlisted()
           .Executes(
                () =>
                {
                    var allHosts = this.GetType()
                                       .GetCustomAttributes<ConfigurationAttributeBase>()
                                       .OfType<IConfigurationGenerator>();

                    allHosts
                       .Select(z => $"""{Assembly.GetEntryAssembly().Location} --{BuildServerConfigurationGeneration.ConfigurationParameterName} {z.Id} --host {z.HostName}""")
                       .ForEach(
                            command => DotNetTasks.DotNet(
                                command,
                                environmentVariables: EnvironmentInfo.Variables.ContainsKey("NUKE_INTERNAL_INTERCEPTOR")
                                    ? EnvironmentInfo.Variables
                                    : new Dictionary<string, string> { ["NUKE_INTERNAL_INTERCEPTOR"] = "1" }.AddDictionary(EnvironmentInfo.Variables)
                            )
                        );
                }
            );
}

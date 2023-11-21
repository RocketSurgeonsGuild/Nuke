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
    internal Target RegenerateBuildConfigurations => t =>
        t
           .TryDependentFor<ICanLintStagedFiles>(static z => z.LintStaged)
           .TryTriggeredBy<ICanLint>(static z => z.Lint)
           .Unlisted()
           .Executes(
                () =>
                {
                    var allHosts = GetType()
                                       .GetCustomAttributes<ConfigurationAttributeBase>()
                                       .OfType<IConfigurationGenerator>();

                    allHosts
                       // ReSharper disable once NullableWarningSuppressionIsUsed
                       .Select(z => $"""{Assembly.GetEntryAssembly()!.Location} --{BuildServerConfigurationGeneration.ConfigurationParameterName} {z.Id} --host {z.HostName}""")
                       .ForEach(
                            command => DotNetTasks.DotNet(
                                command,
                                environmentVariables: EnvironmentInfo.Variables.AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1")
                            )
                        );
                }
            );
}

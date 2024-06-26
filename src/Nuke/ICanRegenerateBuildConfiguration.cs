using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines an interface that can regenerate all of the build configurations
/// </summary>
public interface ICanRegenerateBuildConfiguration : ICanLint
{
    /// <summary>
    ///     Regenerate the build configurations
    /// </summary>
    [UsedImplicitly]
    public Target RegenerateBuildConfigurations =>
        t => t
            .TryDependentFor<ICanLintStagedFiles>(static z => z.LintStaged)
            .TryTriggeredBy<ICanLint>(static z => z.Lint)
             // We run during LintStaged, no need to run again during the lint that lint-staged kicks off.
            .OnlyWhenDynamic(() => !EnvironmentInfo.HasVariable("RSG_NUKE_LINT_STAGED"))
            .Unlisted()
            .Executes(
                 () =>
                 {
                     var allHosts = GetType()
                                   .GetCustomAttributes<ConfigurationAttributeBase>()
                                   .OfType<IConfigurationGenerator>();

                     allHosts
                        .Select(
                             z =>
                                 // ReSharper disable once NullableWarningSuppressionIsUsed
                                 $"""{Assembly.GetEntryAssembly()!.Location} --{BuildServerConfigurationGeneration.ConfigurationParameterName} {z.Id} --host {z.HostName}"""
                         )
                        .ForEach(
                             command => DotNetTasks.DotNet(
                                 command,
                                 environmentVariables: EnvironmentInfo.Variables.AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1")
                             )
                         );
                 }
             );
}
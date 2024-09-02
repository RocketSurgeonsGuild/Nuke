using System.Reflection;
using Nuke.Common.CI;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Rocket.Surgery.Nuke.GithubActions;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines an interface that can regenerate all of the build configurations
/// </summary>
[PublicAPI]
public interface ICanRegenerateBuildConfiguration : ICanLint
{
    /// <summary>
    ///     Regenerate the build configurations
    /// </summary>
    [NonEntryTarget]
    public Target RegenerateBuildConfigurations =>
        t => t
            .TriggeredBy<ICanLint>(static z => z.Lint)
            .Executes(
                 () =>
                 {
                     foreach (var host in GetType()
                                         .GetCustomAttributes<ConfigurationAttributeBase>()
                                         .OfType<IConfigurationGenerator>())
                     {
                         var args = new Arguments()
                                   .Add(Assembly.GetEntryAssembly()!.Location)
                                   .Add($"--{BuildServerConfigurationGeneration.ConfigurationParameterName} {{value}}", host.Id)
                                   .Add("--host {value}", host.HostName);

                         Log.Logger.Information("Regenerating {HostName} configuration id {Name}", host.HostName, host.Id);

                         DotNetTasks.DotNet(
                             args.RenderForExecution(),
                             environmentVariables: EnvironmentInfo.Variables.AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1")                            ,
                             logOutput: false,
                             logInvocation: false
                         );
                     }
                 }
             );
}

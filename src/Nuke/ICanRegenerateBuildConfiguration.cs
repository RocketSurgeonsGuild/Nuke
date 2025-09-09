using System.Reflection;

using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;

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
    Target RegenerateBuildConfigurations =>
        t => t
            .TriggeredBy<ICanLint>(static z => z.Lint)
            .TryAfter<ICanLint>(static z => z.PostLint)
            .Executes(
                 () =>
                 {
                     foreach (var host in GetType()
                                         .GetCustomAttributes<ConfigurationAttributeBase>()
                                         .OfType<IConfigurationGenerator>())
                     {
                         var args = new Arguments()
                                   .Add(AbsolutePath.Create(Assembly.GetEntryAssembly()!.Location))
                                   .Add(
                                        "--{value}",
                                        new Dictionary<string, string>
                                        {
                                            [BuildServerConfigurationGeneration.ConfigurationParameterName] = host.Id,
                                            ["host"] = host.HostName,
                                        },
                                        "{key} {value}"
                                    );

                         Log.Logger.Information("Regenerating {HostName} configuration id {Name}", host.HostName, host.Id);

                         DotNetTasks.DotNet(
                             args.RenderForExecution(),
                             RootDirectory,
                             EnvironmentInfo.Variables.AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1"),
                             logOutput: Verbosity == Verbosity.Verbose,
                             logInvocation: Verbosity == Verbosity.Verbose
                         );
                     }
                 }
             );
}

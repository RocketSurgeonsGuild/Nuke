using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    /// <summary>
    /// Print ci environment with additional variables
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class PrintCIEnvironmentAttribute : BuildExtensionAttributeBase, IOnAfterLogo
    {
        private readonly string[] _additionalPrefixes;

        /// <summary>
        /// Print ci environment with additional variables
        /// </summary>
        /// <param name="additionalPrefixes"></param>
        public PrintCIEnvironmentAttribute(params string[] additionalPrefixes)
        {
            _additionalPrefixes = additionalPrefixes;
        }

        /// <summary>
        /// Well know environment variables
        /// </summary>
        /// <remarks>
        /// Replace default implementation to add values not covered by default
        /// </remarks>
        private static string[] WellKnownEnvironmentVariablePrefixes => new[]
        {
            // Azure pipelines
            "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS", "BITRISE", "BAMBOO", "GITLAB", "JENKINS", "TEAMCITY",
            "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
        };

        /// <inheritdoc />
        public void OnAfterLogo(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> executableTargets,
            IReadOnlyCollection<ExecutableTarget> executionPlan
        )
        {
            if (NukeBuild.IsLocalBuild)
                return;

            using (Logger.Block("CI Environment"))
            {
                Logger.Info("CI: {0}", EnvironmentInfo.GetVariable<string>("CI"));

                foreach (var variable in WellKnownEnvironmentVariablePrefixes.Concat(_additionalPrefixes)
                   .SelectMany(
                        prefix => EnvironmentInfo.Variables.Keys.Where(
                            key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        )
                    ))
                {
                    Logger.Info($"{variable}: {EnvironmentInfo.Variables[variable]}");
                }
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Logger;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class PrintBuildVersionAttribute : Attribute, IOnAfterLogo
    {
        /// <inheritdoc />
        public void OnAfterLogo(
            NukeBuild build,
            IReadOnlyCollection<ExecutableTarget> executableTargets,
            IReadOnlyCollection<ExecutableTarget> executionPlan
        )
        {
            if (build is IHaveGitVersion gitVersion && build is IHaveSolution solution &&
                build is IHaveConfiguration configuration)
            {
                using (Block("Build Version"))
                {
                    Info(
                        "Building version {0} of {1} ({2}) using version {3} of Nuke.",
                        gitVersion.GitVersion?.InformationalVersion,
                        solution.Solution.Name,
                        configuration.Configuration,
                        typeof(NukeBuild).Assembly.GetVersionText()
                    );
                }
            }
        }
    }

    /// <summary>
    /// Print ci environment with additional variables
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class PrintCIEnvironmentAttribute : Attribute, IOnAfterLogo
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

            using (Block("CI Environment"))
            {
                Info("CI: {0}", GetVariable<string>("CI"));

                foreach (var variable in WellKnownEnvironmentVariablePrefixes.Concat(_additionalPrefixes)
                   .SelectMany(
                        prefix => Variables.Keys.Where(
                            key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        )
                    ))
                {
                    Info($"{variable}: {Variables[variable]}");
                }
            }
        }
    }

    /// <summary>
    /// Dumps ci state for debug purposes
    /// </summary>
    public interface ICIEnvironment : IHaveBuildVersion
    {
        /// <summary>
        /// Well know environment variables
        /// </summary>
        /// <remarks>
        /// Replace default implementation to add values not covered by default
        /// </remarks>
        public IEnumerable<string> WellKnownEnvironmentVariablePrefixes => new[]
        {
            // Azure pipelines
            "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS", "BITRISE", "BAMBOO", "GITLAB", "JENKINS", "TEAMCITY",
            "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
        };

        /// <summary>
        /// Prints CI environment state for debug purposes
        /// </summary>
        public Target CIEnvironment => _ => _
           .TriggeredBy(BuildVersion)
           .OnlyWhenStatic(() => NukeBuild.IsServerBuild)
           .Executes(
                () =>
                {
                    Info("CI: {0}", GetVariable<string>("CI"));

                    foreach (var variable in WellKnownEnvironmentVariablePrefixes
                       .SelectMany(
                            prefix => Variables.Keys.Where(
                                key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            )
                        ))
                    {
                        Info($"{variable}: {Variables[variable]}");
                    }
                }
            );
    }
}
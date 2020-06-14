﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
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
                    Logger.Info("CI: {0}", EnvironmentInfo.GetVariable<string>("CI"));

                    foreach (var variable in WellKnownEnvironmentVariablePrefixes
                       .SelectMany(
                            prefix => EnvironmentInfo.Variables.Keys.Where(
                                key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            )
                        ))
                    {
                        Logger.Info($"{variable}: {EnvironmentInfo.Variables[variable]}");
                    }
                }
            );
    }
}
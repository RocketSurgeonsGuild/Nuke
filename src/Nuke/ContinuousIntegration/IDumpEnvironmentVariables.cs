using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using static Nuke.Common.Logger;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.ContinuousIntegration
{
    public interface IPrintCIEnvironment : IHaveBuildVersion
    {
        public IEnumerable<string> WellKnownEnvironmentVariablePrefixes => new[]
        {
            // Azure pipelines
            "CIRCLE", "GITHUB", "APPVEYOR", "TRAVIS",
            "AGENT_", "BUILD_", "RELEASE_", "PIPELINE_", "ENVIRONMENT_", "SYSTEM_",
        };

        public Target PrintCIEnvironment => _ => _
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
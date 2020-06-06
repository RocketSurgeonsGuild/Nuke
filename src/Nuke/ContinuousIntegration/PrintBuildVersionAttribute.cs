using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;
using static Nuke.Common.Logger;

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
}
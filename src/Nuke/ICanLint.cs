using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Temp.CleanupCode;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Adds support for linting the files in a solution or via
    /// </summary>
    public interface ICanLint : IHaveSolution
    {
        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The files to lint, if not given lints all files", Separator = " ")]
        public IEnumerable<string> LintFiles => InjectionUtility.GetInjectionValue(() => LintFiles ?? Array.Empty<string>());

        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The profile to use for linting")]
        public string LintProfile => InjectionUtility.GetInjectionValue(() => LintProfile ?? "Full Cleanup");

        /// <summary>
        /// Applies code cleanup tasks
        /// </summary>
        public Target Lint => _ => _
           .Requires(() => LintFiles)
           .Executes(
                () =>
                {
                    CleanupCodeTasks.CleanupCode(
                        x => CleanupCodeSettingsExtensions.SetTargetPath(x, Solution.Path)
                           .SetProfile(LintProfile)
                           .AddInclude(LintFiles)
                    );
                }
            );
    }
}
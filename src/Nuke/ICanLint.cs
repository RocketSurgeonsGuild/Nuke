using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ValueInjection;
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
        public string[] LintFiles => ValueInjectionUtility.TryGetValue(() => LintFiles) ?? Array.Empty<string>();

        /// <summary>
        /// The files to lint, if not given lints all files
        /// </summary>
        [Parameter("The profile to use for linting")]
        public string LintProfile => ValueInjectionUtility.TryGetValue(() => LintProfile) ?? "Full Cleanup";

        /// <summary>
        /// Applies code cleanup tasks
        /// </summary>
        public Target Lint => _ => _
           .Requires(() => LintFiles)
           .Executes(
                () =>
                {
                    CleanupCodeTasks.CleanupCode(
                        x => x.SetTargetPath(Solution.Path)
                           .SetProfile(LintProfile)
                           .AddInclude(LintFiles)
                    );
                }
            );
    }
}
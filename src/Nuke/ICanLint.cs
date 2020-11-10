using Microsoft.VisualBasic;
using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.ReSharper;
using Nuke.Common.ValueInjection;
using static Nuke.Common.Tools.ReSharper.ReSharperTasks;

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
                    Logger.Info(string.Join(", ", LintFiles));
                    ReSharperCleanupCode(
                        x => x.SetTargetPath(Solution.Path)
                           .SetProcessToolPath(ToolPathResolver.GetPackageExecutable("JetBrains.ReSharper.GlobalTools", "JetBrains.CommandLine.Products.dll"))
                           .SetProfile(LintProfile)
                           .AddInclude(LintFiles)
                    );
                }
            );
    }
}
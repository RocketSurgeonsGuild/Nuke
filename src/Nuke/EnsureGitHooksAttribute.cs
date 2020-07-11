using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using static Nuke.Common.IO.FileSystemTasks;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Ensures that the given git hooks are defined in the .git directory
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class EnsureGitHooksAttribute : BuildExtensionAttributeBase, IOnBeforeLogo
    {
        /// <summary>
        /// Ensures that the given git hooks are defined in the .git directory
        /// </summary>
        public EnsureGitHooksAttribute(params GitHook[] hookNames)
        {
#pragma warning disable CA1307, CA1308
            HookNames = hookNames.Select(x => x.ToString().Humanize().Replace(" ", "_").Dasherize().ToLowerInvariant())
               .ToArray();
#pragma warning restore CA1307, CA1308
        }

        /// <summary>
        /// The hookes that were asked for.
        /// </summary>
        public string[] HookNames { get; }

        /// <inheritdoc />
        public void OnBeforeLogo(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
        {
            if (!NukeBuild.IsLocalBuild)
            {
                return;
            }
            // We only care on local machines

            if (HookNames.Any(hook => !FileExists(NukeBuild.RootDirectory / $".git/hooks/{hook}")))
            {
                Logger.Info("Git hooks not found...");

                if (FileExists(NukeBuild.RootDirectory / "package.json"))
                {
                    Logger.Info("package.json found running npm install to see if that installs any hooks");
                    ProcessTasks.StartProcess(ToolPathResolver.GetPathExecutable("npm"), "install").AssertWaitForExit()
                       .AssertZeroExitCode();
                }
            }

            foreach (var hook in HookNames)
            {
                if (!FileExists(NukeBuild.RootDirectory / $".git/hooks/{hook}"))
                {
                    Logger.Info($"Was unable to install {hook} hook.");
                }
            }
        }
    }
}
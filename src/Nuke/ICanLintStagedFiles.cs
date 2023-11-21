using System.Reflection;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Rocket.Surgery.Nuke.DotNetCore;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Allows the build to lint staged files, as well as lint files from a given pull request.
/// </summary>
/// <remarks>
/// uses the lint-staged npm package under the covers, with dotnet nuke lint-staged as the entry point.
/// </remarks>
public interface ICanLintStagedFiles : INukeBuild
{
    /// <summary>
    /// Run staged lint tasks
    /// </summary>
    public Target LintStaged => t =>
        t
           .Executes(
                () => ProcessTasks.StartProcess(
                    ToolPathResolver.GetPathExecutable("npx"),
                    GitHubActions.Instance.IsPullRequest() ? $"lint-staged -r --diff=\"origin/{GitHubActions.Instance.BaseRef}...origin/{GitHubActions.Instance.HeadRef}\"" : "lint-staged -r",
                    environmentVariables: EnvironmentInfo.Variables
                                                         .AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1")
                                                         .ReplaceIfSet("TERM", "dumb") // ensure lint staged doesn't try to use it's fancy renderer
                                                         .ReplaceIfSet("LISTR_FORCE_UNICODE", "1")
                                                         .ReplaceIfSet("FORCE_COLOR", "1")
                                                         // ReSharper disable once NullableWarningSuppressionIsUsed
                                                         .AddIfMissing("NUKE_BUILD_ASSEMBLY", RootDirectory.GetRelativePathTo(Assembly.GetEntryAssembly()!.Location)),
                    logOutput: true,
                    logger: (type, s) =>
                    {
                        if (type == OutputType.Std)
                        {
                            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                            Console.Out.WriteLine(s);
                        }
                        else
                        {
                            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                            Console.Error.WriteLine(s);
                        }
                    }
                ).AssertWaitForExit().AssertZeroExitCode()
            )
           .Executes(
                () =>
                {
                    GitTasks.Git("add .nuke/build.schema.json");
                    GitTasks.Git("add .github/workflows/*.yml");
                    if (this is IHavePublicApis)
                    {
                        GitTasks.Git("add *PublicAPI.Shipped.txt *PublicAPI.Unshipped.txt");
                    }
                }
            );
}

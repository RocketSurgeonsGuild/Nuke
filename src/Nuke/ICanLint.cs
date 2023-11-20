using System.Reflection;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
public interface ICanLint : INukeBuild
{
    /// <summary>
    ///     The lint target
    /// </summary>
    public Target Lint => t => t;

    /// <summary>
    ///     A lint target that runs last
    /// </summary>
    public Target PostLint => t => t.Unlisted().After(Lint).TriggeredBy(Lint);

    /// <summary>
    ///     The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ", Name = "lint-files")]
#pragma warning disable CA1819
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? Array.Empty<string>();
#pragma warning restore CA1819

    /// <summary>
    ///     The lint paths rooted as an absolute path.
    /// </summary>
    protected internal IEnumerable<AbsolutePath> LintPaths => PrivateLintFiles.Select(z => Path.IsPathRooted(z) ? (AbsolutePath)z : RootDirectory / z);
}

/// <summary>
/// Allows the build to lint staged files, as well as lint files from a given pull request.
/// </summary>
/// <remarks>
/// uses the lint-staged npm package under the covers, with dotnet nuke lint-staged as the entry point.
/// </remarks>
public interface ICanLintStagedFiles : ICanRegenerateBuildConfiguration, INukeBuild
{
    /// <summary>
    /// Run staged lint tasks
    /// </summary>
    public Target LintStaged => t =>
        t
           .DependsOn(RegenerateBuildConfigurations)
           .OnlyWhenDynamic(() => LintStagedIsPullRequest || IsLocalBuild)
           .Executes(
                () => ProcessTasks.StartProcess(
                    ToolPathResolver.GetPathExecutable("npx"),
                    $"lint-staged -r {( LintStagedIsPullRequest ? $"""--diff="origin/{GitHubActions.Instance.BaseRef}...origin/{GitHubActions.Instance.HeadRef}" """ : "" )}",
                    environmentVariables: EnvironmentInfo.Variables
                                                         .AddIfMissing("NUKE_INTERNAL_INTERCEPTOR", "1")
                                                          // ReSharper disable once NullableWarningSuppressionIsUsed
                                                         .AddIfMissing("NUKE_BUILD_ASSEMBLY", RootDirectory.GetRelativePathTo(Assembly.GetEntryAssembly()!.Location)),
                    logOutput: true,
                    logger: (type, s) =>
                    {
                        if (type == OutputType.Std)
                        {
                            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                            Log.Information(s);
                        }
                        else
                        {
                            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                            Log.Error(s);
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

    /// <summary>
    /// Defines if the current environment is a pull request
    /// </summary>
    public bool LintStagedIsPullRequest => GitHubActions.Instance?.IsPullRequest == true;
}

using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Serilog;

#pragma warning disable CA1019
#pragma warning disable CA1308
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Ensures that the given git hooks are defined in the .git directory
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class EnsureGitHooksAttribute : BuildExtensionAttributeBase, IOnBuildCreated, IOnBuildInitialized
{
    /// <summary>
    ///     Ensures that the given git hooks are defined in the .git directory
    /// </summary>
    public EnsureGitHooksAttribute(params GitHook[] hookNames)
    {
        HookNames = hookNames
                   .Select(
                        x => x
                            .ToString()
                            .Humanize()
                            .Replace(" ", "_", StringComparison.Ordinal)
                            .Dasherize()
                            .ToLowerInvariant()
                    )
                   .ToArray();
    }

    /// <summary>
    ///     The hooks that were asked for.
    /// </summary>
    public string[] HookNames { get; }

    /// <inheritdoc />
    public void OnBuildCreated(IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        // Only care about local environments
        if (!NukeBuild.IsLocalBuild) return;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (Build is IGitHooksEngine engine)
        {
            installHooks(engine, HookNames);
        }

        if (( Build.RootDirectory / ".husky" ).DirectoryExists())
        {
            engine = new HuskyEngine();
            installHooks(engine, HookNames);
        }

        static void installHooks(IGitHooksEngine engine, string[] hookNames)
        {
            if (!engine.AreHooksInstalled(hookNames))
            {
                Log.Information("git hooks not found...");
                engine.InstallHooks(hookNames);
            }
        }
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        if (( NukeBuild.RootDirectory / "package.json" ).FileExists() && !NukeBuild.RootDirectory.ContainsDirectory("node_modules"))
        {
            ProcessTasks
               .StartProcess(ToolPathResolver.GetPathExecutable("npm"), NukeBuild.IsLocalBuild ? "install" : "ci --ignore-scripts", NukeBuild.RootDirectory)
               .AssertWaitForExit()
               .AssertZeroExitCode();
        }
    }
}

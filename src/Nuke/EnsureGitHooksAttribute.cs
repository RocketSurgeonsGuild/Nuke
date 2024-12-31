using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

using Serilog;

#pragma warning disable CA1019, CA1308
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Ensures that the given git hooks are defined in the .git directory
/// </summary>
/// <remarks>
///     Ensures that the given git hooks are defined in the .git directory
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class EnsureGitHooksAttribute(params GitHook[] hookNames) : BuildExtensionAttributeBase, IOnBuildCreated, IOnBuildInitialized
{
    /// <summary>
    ///     The hooks that were asked for.
    /// </summary>
    public string[] HookNames { get; } = hookNames
                   .Select(
                        x => x
                            .ToString()
                            .Humanize()
                            .Replace(" ", "_", StringComparison.Ordinal)
                            .Dasherize()
                            .ToLowerInvariant()
                    )
                   .ToArray();

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

        if (!( Build.RootDirectory / ".husky" ).DirectoryExists())
        {
            return;
        }

        engine = new HuskyEngine();
        installHooks(engine, HookNames);

        static void installHooks(IGitHooksEngine engine, string[] hookNames)
        {
            if (engine.AreHooksInstalled(hookNames))
            {
                return;
            }

            Log.Information("git hooks not found...");
            engine.InstallHooks(hookNames);
        }
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        if (!( NukeBuild.RootDirectory / "package.json" ).FileExists() || NukeBuild.RootDirectory.ContainsDirectory("node_modules"))
        {
            return;
        }

        ProcessTasks
           .StartProcess(ToolPathResolver.GetPathExecutable("npm"), NukeBuild.IsLocalBuild ? "install" : "ci --ignore-scripts", NukeBuild.RootDirectory)
           .AssertWaitForExit()
           .AssertZeroExitCode();
    }
}

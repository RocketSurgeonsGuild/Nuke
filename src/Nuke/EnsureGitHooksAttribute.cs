using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
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
        if (Build is not IGitHooksEngine engine)
        {
            Log.Verbose("No git hooks engine found, defaulting to husky");
            engine = new HuskyEngine();
        }

        if (!engine.AreHooksInstalled(HookNames))
        {
            Log.Information("git hooks not found...");
            engine.InstallHooks(HookNames);
        }
    }

    /// <inheritdoc />
    public void OnBuildInitialized(IReadOnlyCollection<ExecutableTarget> executableTargets, IReadOnlyCollection<ExecutableTarget> executionPlan)
    {
        if (( NukeBuild.RootDirectory / "package.json" ).FileExists() && !NukeBuild.RootDirectory.ContainsDirectory("node_modules"))
        {
            Log.Information("package.json found running npm install to see if that installs any hooks");
            ProcessTasks
               .StartProcess(ToolPathResolver.GetPathExecutable("npm"), NukeBuild.IsLocalBuild ? "install" : "ci --ignore-scripts", NukeBuild.RootDirectory)
               .AssertWaitForExit()
               .AssertZeroExitCode();
        }
    }

    private class HuskyEngine : IGitHooksEngine
    {
        public bool AreHooksInstalled(IReadOnlyCollection<string> hooks)
        {
            if (NukeBuild.IsServerBuild) return true;
            try
            {
                var hooksOutput = GitTasks.Git($"config --get core.hookspath", logOutput: false, logInvocation: false);
                var hooksPath = hooksOutput.StdToText().Trim();
                var huskyScriptPath = NukeBuild.RootDirectory / ".husky" / "_" / "husky.sh";
                return hooksPath.StartsWith(".husky") && huskyScriptPath.FileExists();
            }
            #pragma warning disable CA1031
            catch
                #pragma warning restore CA1031
            {
                return false;
            }
        }

        public void InstallHooks(IReadOnlyCollection<string> hooks)
        {
            if (( NukeBuild.RootDirectory / "package.json" ).FileExists())
            {
                Log.Information("package.json found running npm install to see if that installs any hooks");
                ProcessTasks
                   .StartProcess(ToolPathResolver.GetPathExecutable("npm"), "install", NukeBuild.RootDirectory)
                   .AssertWaitForExit()
                   .AssertZeroExitCode();
                if (NukeBuild.IsLocalBuild)
                    ProcessTasks
                       .StartProcess(ToolPathResolver.GetPathExecutable("npm"), "run prepare", NukeBuild.RootDirectory)
                       .AssertWaitForExit();
            }

            if (!AreHooksInstalled(hooks))
            {
                Log.Information(
                    "package.json not found or prepare script did not work correctly running npx husky"
                );
                ProcessTasks
                   .StartProcess(ToolPathResolver.GetPathExecutable("npx"), "husky", NukeBuild.RootDirectory)
                   .AssertWaitForExit()
                   .AssertZeroExitCode();
            }
        }
    }
}
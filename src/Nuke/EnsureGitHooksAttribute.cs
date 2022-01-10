using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using static Nuke.Common.IO.FileSystemTasks;

#pragma warning disable CA1019
#pragma warning disable CA1308
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Ensures that the given git hooks are defined in the .git directory
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class EnsureGitHooksAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    /// <summary>
    ///     Ensures that the given git hooks are defined in the .git directory
    /// </summary>
    public EnsureGitHooksAttribute(params GitHook[] hookNames)
    {
        HookNames = hookNames
                   .Select(x => x.ToString().Humanize().Replace(" ", "_", StringComparison.Ordinal).Dasherize().ToLowerInvariant())
                   .ToArray();
    }

    /// <summary>
    ///     The hooks that were asked for.
    /// </summary>
    public string[] HookNames { get; }

    /// <inheritdoc />
    public void OnBuildCreated(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        if (!NukeBuild.IsLocalBuild)
        {
            return;
        }
        // We only care on local machines

        if (HookNames.Any(hook => !(NukeBuild.RootDirectory / $".git/hooks/{hook}").FileExists())
         || !(NukeBuild.RootDirectory / "node_modules").DirectoryExists())
        {
            Serilog.Log.Information("Git hooks not found...");

            if ((NukeBuild.RootDirectory / "package.json").FileExists())
            {
                Serilog.Log.Information("package.json found running npm install to see if that installs any hooks");
                ProcessTasks.StartProcess(ToolPathResolver.GetPathExecutable("npm"), "install").AssertWaitForExit()
                            .AssertZeroExitCode();
            }
        }

        foreach (var hook in HookNames)
        {
            if (!(NukeBuild.RootDirectory / $".git/hooks/{hook}").FileExists())
            {
                Serilog.Log.Information("Was unable to install {Hook} hook", hook);
            }
        }
    }
}

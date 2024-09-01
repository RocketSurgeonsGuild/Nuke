using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;

namespace Rocket.Surgery.Nuke;

class HuskyEngine : IGitHooksEngine
{
    public bool AreHooksInstalled(IReadOnlyCollection<string> hooks)
    {
        if (NukeBuild.IsServerBuild) return true;
        try
        {
            var hooksOutput = GitTasks.Git($"config --get core.hookspath", logOutput: false, logInvocation: false);
            var hooksPath = hooksOutput.StdToText().Trim();
            return hooksPath.StartsWith(".husky");
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
        if (!AreHooksInstalled(hooks))
        {
            DotNetTool.GetTool("husky")("install");
        }
    }
}

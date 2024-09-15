using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;

namespace Rocket.Surgery.Nuke;

internal class HuskyEngine : IGitHooksEngine
{
    public bool AreHooksInstalled(IReadOnlyCollection<string> hooks)
    {
        if (NukeBuild.IsServerBuild)
        {
            return true;
        }

        try
        {
            var hooksOutput = GitTasks.Git("config --get core.hookspath", logOutput: false, logInvocation: false);
            var hooksPath = hooksOutput.StdToText().Trim();

            return hooksPath.StartsWith(".husky") && AbsolutePath.Create(NukeBuild.RootDirectory / hooksPath / "_" / "husky.sh").FileExists();
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
        if (AreHooksInstalled(hooks))
        {
            return;
        }

        _ = DotNetTool.GetTool("husky")("install");
    }
}

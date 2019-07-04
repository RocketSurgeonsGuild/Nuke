using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Program : DotNetCoreBuild
{
    /// <summary>
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>

    public static int Main() => Execute<Program>(x => x.Default);

    Target Default => _ => _.DependsOn(Core);
}

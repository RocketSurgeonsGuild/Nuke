using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke;
using JetBrains.Annotations;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
[PublicAPI]
class Solution : DotNetCoreBuild, IDotNetCoreBuild
{
    /// <summary>
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    /// </summary>

    public static int Main() => Execute<Solution>(x => x.Default);

    Target Default => _ => _
        .DependsOn(Restore)
        .DependsOn(Build)
        .DependsOn(Test)
        .DependsOn(Pack)
        ;

    public new Target Restore => _ => _.With(DotNetCoreBuild.Restore, this);

    public new Target Build => _ => _.With(DotNetCoreBuild.Build, this);

    public new Target Test => _ => _.With(DotNetCoreBuild.Test, this);

    public new Target Pack => _ => _.With(DotNetCoreBuild.Pack, this);
}

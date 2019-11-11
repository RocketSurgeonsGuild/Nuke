using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke;
using JetBrains.Annotations;

[PublicAPI]
[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
[PackageIcon("https://raw.githubusercontent.com/RocketSurgeonsGuild/graphics/master/png/social-square-thrust-rounded.png")]
[EnsurePackageSourceHasCredentials("RocketSurgeonsGuild")]
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

    public new Target Restore => _ => _.With(this, DotNetCoreBuild.Restore);

    public new Target Build => _ => _.With(this, DotNetCoreBuild.Build);

    public new Target Test => _ => _.With(this, DotNetCoreBuild.Test);

    public new Target Pack => _ => _.With(this, DotNetCoreBuild.Pack);
}

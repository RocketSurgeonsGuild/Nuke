using System;
using Nuke.Common;
using Nuke.Common.Execution;
using Rocket.Surgery.Nuke;

namespace Rocket.Surgery.Nuke.Xamarin
{
    [CheckBuildProjectConfigurations]
    [UnsetVisualStudioEnvironmentVariables]
    public class MacBuild : XamarinBuild
    {
        public Target XamarinMac => _ => _
            .DependsOn(Clean)
            .DependsOn(Restore)
            .DependsOn(Build)
            .DependsOn(Test);
    }
}

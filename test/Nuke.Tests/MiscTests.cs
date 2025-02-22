using Nuke.Common.Tools.NuGet;

using Rocket.Surgery.Extensions.Testing;

using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Nuke.Tests;

public class MiscTests(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitTestContext.Create(outputHelper))
{
    [Fact]
    public void Test1()
    {
        var attr = new EnsurePackageSourceHasCredentialsAttribute("Source");
        attr.SourceName.ShouldBe("Source");
    }

    [Fact]
    public void NuGetVerbosityMappingAttribute()
    {
        var attr = new NuGetVerbosityMappingAttribute();
        attr.Quiet.ShouldBe(nameof(NuGetVerbosity.Quiet));
        attr.Minimal.ShouldBe(nameof(NuGetVerbosity.Normal));
        attr.Normal.ShouldBe(nameof(NuGetVerbosity.Normal));
        attr.Verbose.ShouldBe(nameof(NuGetVerbosity.Detailed));
    }
}

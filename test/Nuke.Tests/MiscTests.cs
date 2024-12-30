using FluentAssertions;
using Nuke.Common.Tools.NuGet;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Nuke.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class MiscTests(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitTestContext.Create(outputHelper))
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    [Fact]
    public void Test1()
    {
        var attr = new EnsurePackageSourceHasCredentialsAttribute("Source");
        attr.SourceName.Should().Be("Source");
    }

    [Fact]
    public void NuGetVerbosityMappingAttribute()
    {
        var attr = new NuGetVerbosityMappingAttribute();
        attr.Quiet.Should().Be(nameof(NuGetVerbosity.Quiet));
        attr.Minimal.Should().Be(nameof(NuGetVerbosity.Normal));
        attr.Normal.Should().Be(nameof(NuGetVerbosity.Normal));
        attr.Verbose.Should().Be(nameof(NuGetVerbosity.Detailed));
    }
}

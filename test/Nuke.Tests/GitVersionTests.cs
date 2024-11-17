using FluentAssertions;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tools.NuGet;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.Tests;

public class GitVersionTests : AutoFakeTest
{
    [Fact]
    public void Fact1()
    {
        SetVariable("GITVERSION_SomeOtherValue", "someValue");

        ComputedGitVersionAttribute.HasGitVer().Should().BeTrue();
    }

    public GitVersionTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Trace) { }
}

public class MiscTests : AutoFakeTest
{
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

    public MiscTests(ITestOutputHelper outputHelper) : base(outputHelper) { }
}

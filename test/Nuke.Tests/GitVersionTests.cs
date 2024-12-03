using FluentAssertions;
using Rocket.Surgery.Extensions.Testing;
using Xunit;
using Xunit.Abstractions;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class GitVersionTests(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitTestContext.Create(outputHelper))
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Fact]
    public void Fact1()
    {
        SetVariable("GITVERSION_SomeOtherValue", "someValue");

        _ = ComputedGitVersionAttribute.HasGitVer().Should().BeTrue();
    }
}

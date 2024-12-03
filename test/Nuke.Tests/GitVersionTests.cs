using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.Tests;

public class GitVersionTests(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitTestContext.Create(outputHelper))
{
    [Fact]
    public void Fact1()
    {
        SetVariable("GITVERSION_SomeOtherValue", "someValue");

        ComputedGitVersionAttribute.HasGitVer().Should().BeTrue();
    }
}

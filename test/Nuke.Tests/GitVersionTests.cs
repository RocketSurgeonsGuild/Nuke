using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing;
using Rocket.Surgery.Nuke;
using Xunit;
using Xunit.Abstractions;

using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke.Tests
{
    public class GitVersionTests : AutoFakeTest
    {
        public GitVersionTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Trace)
        {
        }

        [Fact]
        public void Fact1()
        {
            SetVariable("GITVERSION_SomeOtherValue", "someValue");

            ComputedGitVersionAttribute.HasGitVer().Should().BeTrue();
        }
    }
}

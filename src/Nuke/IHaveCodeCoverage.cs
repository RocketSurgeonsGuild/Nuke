using Nuke.Common.IO;
using Nuke.Common.Tools.DotCover;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds a code coverage directory
/// </summary>
/// <remarks>
///     This directory is left separate to allow easier integration with editors that might look it's contents to display
///     coverage.
/// </remarks>
public interface IHaveCodeCoverage : IHaveArtifacts
{
    /// <summary>
    ///     The directory where coverage artifacts are to be dropped
    /// </summary>
    [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
    public AbsolutePath CoverageDirectory =>
        EnvironmentInfo.GetVariable<AbsolutePath>("Coverage")
     ?? TryGetValue(() => CoverageDirectory)
     ?? NukeBuild.RootDirectory / "coverage";

    public static IEnumerable<string> DefaultDotCoverAttributeFilters { get; } =
    [
        "System.Diagnostics.DebuggerHiddenAttribute",
        "System.Diagnostics.DebuggerNonUserCodeAttribute",
        "System.CodeDom.Compiler.GeneratedCodeAttribute",
        "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
    ];

    public static IEnumerable<string> DefaultDotCoverFilters { get; } =
    [
        "-:Bogus*",
        "-:FakeItEasy*",
        "-:Moq*",
        "-:NSubstitute*",
        "-:Verify*",
        "-:XUnit*",
        "-:TUnit*",
        "-:Microsoft*",
        "-:System*",
        "-:JetBrains*",
        "-:DryIoc*",
        "-:Nuke*",
        "-:testhost*",
        "-:FluentAssertions*",
        "-:Serilog*",
        "-:module=JetBrains*",
        "-:class=JetBrains*"
    ];

    public DotCoverCoverDotNetSettings CustomizeDotCoverSettings(DotCoverCoverDotNetSettings settings)
    {
        return settings;
    }
}

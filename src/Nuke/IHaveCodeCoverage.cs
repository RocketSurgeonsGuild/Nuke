using Nuke.Common.IO;

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

    public IEnumerable<string> IncludeModulePaths => [];
    public IEnumerable<string> ExcludeModulePaths => [];
    public IEnumerable<string> IncludeAttributes => [];

    public IEnumerable<string> ExcludeAttributes =>
    [
        "System.Diagnostics.DebuggerHiddenAttribute",
        "System.Diagnostics.DebuggerNonUserCodeAttribute",
        "System.CodeDom.Compiler.GeneratedCodeAttribute",
        "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
    ];

    public IEnumerable<string> IncludeNamespaces => [];

    public IEnumerable<string> ExcludeNamespaces =>
    [
        "Bogus.",
        "FakeItEasy.",
        "Moq.",
        "NSubstitute.",
        "Verify.",
        "XUnit.",
        "TUnit.",
        "Microsoft.",
        "System.",
        "JetBrains.",
        "DryIoc.",
        "Nuke.",
        "FluentAssertions.",
        "Serilog.",
    ];
}

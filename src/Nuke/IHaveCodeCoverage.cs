using System.Xml.Linq;

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
    XDocument CustomizeCoverageRunSettings(XDocument document) => document;
    static IEnumerable<string> DefaultIncludeModulePaths => [];
    static IEnumerable<string> DefaultExcludeModulePaths => [];
    static IEnumerable<string> DefaultIncludeSources => [];
    static IEnumerable<string> DefaultExcludeSources => [];
    static IEnumerable<string> DefaultIncludeAttributes => [];

    static IEnumerable<string> DefaultExcludeAttributes =>
    [
        "System.Diagnostics.DebuggerHiddenAttribute",
        "System.Diagnostics.DebuggerNonUserCodeAttribute",
        "System.CodeDom.Compiler.GeneratedCodeAttribute",
        "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
    ];

    static IEnumerable<string> DefaultIncludeNamespaces => [];

    static IEnumerable<string> DefaultExcludeNamespaces =>
    [
        "Bogus",
        "FakeItEasy",
        "Moq",
        "NSubstitute",
        "Verify",
        "XUnit",
        "TUnit",
        "Microsoft.",
        "System.",
        "JetBrains.",
        "DryIoc",
        "Nuke",
        "FluentAssertions",
        "Serilog",
    ];

    /// <summary>
    ///     The directory where coverage artifacts are to be dropped
    /// </summary>
    [Parameter("The directory where coverage artifacts are to be dropped", Name = "Coverage")]
    AbsolutePath CoverageDirectory =>
        EnvironmentInfo.GetVariable<AbsolutePath>("Coverage")
     ?? TryGetValue(() => CoverageDirectory)
     ?? NukeBuild.RootDirectory / "coverage";

    IEnumerable<string> IncludeNamespaces => DefaultIncludeNamespaces;
    IEnumerable<string> ExcludeNamespaces => DefaultExcludeNamespaces;
    IEnumerable<string> IncludeAttributes => DefaultIncludeAttributes;
    IEnumerable<string> ExcludeAttributes => DefaultExcludeAttributes;
    IEnumerable<string> IncludeSources => DefaultIncludeSources;
    IEnumerable<string> ExcludeSources => DefaultExcludeSources;
    IEnumerable<string> IncludeModulePaths => DefaultIncludeModulePaths;
    IEnumerable<string> ExcludeModulePaths => DefaultExcludeModulePaths;
}

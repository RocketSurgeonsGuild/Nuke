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
    public static IEnumerable<string> DefaultIncludeModulePaths => [];
    public static IEnumerable<string> DefaultExcludeModulePaths => [];
    public static IEnumerable<string> DefaultIncludeSources => [];
    public static IEnumerable<string> DefaultExcludeSources => [];
    public static IEnumerable<string> DefaultIncludeAttributes => [];

    public static IEnumerable<string> DefaultExcludeAttributes =>
    [
        "System.Diagnostics.DebuggerHiddenAttribute",
        "System.Diagnostics.DebuggerNonUserCodeAttribute",
        "System.CodeDom.Compiler.GeneratedCodeAttribute",
        "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
        "System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute",
    ];

    public static IEnumerable<string> DefaultIncludeNamespaces => [];

    public static IEnumerable<string> DefaultExcludeNamespaces =>
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
    public AbsolutePath CoverageDirectory =>
        EnvironmentInfo.GetVariable<AbsolutePath>("Coverage")
     ?? TryGetValue(() => CoverageDirectory)
     ?? NukeBuild.RootDirectory / "coverage";

    public IEnumerable<string> IncludeNamespaces => DefaultIncludeNamespaces;
    public IEnumerable<string> ExcludeNamespaces => DefaultExcludeNamespaces;
    public IEnumerable<string> IncludeAttributes => DefaultIncludeAttributes;
    public IEnumerable<string> ExcludeAttributes => DefaultExcludeAttributes;
    public IEnumerable<string> IncludeSources => DefaultIncludeSources;
    public IEnumerable<string> ExcludeSources => DefaultExcludeSources;
    public IEnumerable<string> IncludeModulePaths => DefaultIncludeModulePaths;
    public IEnumerable<string> ExcludeModulePaths => DefaultExcludeModulePaths;

    public XDocument CustomizeCoverageRunSettings(XDocument document) => document;
}

using System.Diagnostics;
using Buildalyzer;
using Buildalyzer.Construction;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke.ProjectModel;

/// <summary>
///     A wrapper around the analyzer result to provide a more strongly typed model
/// </summary>
/// <remarks>
///     A wrapper around the analyzer result to provide a more strongly typed model
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ProjectAnalyzerModel(IAnalyzerResult result) : IAnalyzerResult
{
    /// <summary>
    ///     Implicitly convert the model to the project file path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static implicit operator string(ProjectAnalyzerModel model)
    {
        return model.ProjectFilePath;
    }

    /// <summary>
    ///     The project name
    /// </summary>
    public string Name => ProjectFile.Name;

    /// <summary>
    ///     The project file model
    /// </summary>
    public IProjectFile ProjectFile => Project.ProjectFile;

    /// <summary>
    ///     The project file model
    /// </summary>
    public ProjectAnalyzer Project { get; } = result.Analyzer;

    /// <summary>
    ///     The package id
    /// </summary>

    public string PackageId => GetProperty<string>(nameof(PackageId)) ?? ProjectFile.Name;

    /// <summary>
    ///     Is this project packable
    /// </summary>

    public bool IsPackable => GetProperty<bool>(nameof(IsPackable));

    /// <summary>
    ///     Is this a test project
    /// </summary>
    public bool IsTestProject => GetProperty<bool>(nameof(IsTestProject));

    /// <summary>
    ///     The project file path
    /// </summary>
    public AbsolutePath ProjectFilePath => result.ProjectFilePath;

    /// <summary>
    ///     The directory of the project file
    /// </summary>
    public AbsolutePath Directory => ProjectFilePath.Parent!;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    /// <summary>
    ///     Get a property from the analyzer result supports a limited number of types
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public T? GetProperty<T>(string name) where T : notnull
    {
        var value = result.GetProperty(name);
        return typeof(T) == typeof(bool)
            ? (T?)(object?)( value is "enable" or "true" )
            : typeof(T) == typeof(string)
                ? (T?)(object?)value
                : throw new NotSupportedException(typeof(T).FullName);
    }

    string IAnalyzerResult.GetProperty(string name)
    {
        return result.GetProperty(name);
    }

    /// <summary>
    ///     The source project analyzer
    /// </summary>
    public ProjectAnalyzer Analyzer => result.Analyzer;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IProjectItem[]> Items => result.Items;

    AnalyzerManager IAnalyzerResult.Manager => result.Manager;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> PackageReferences => result.PackageReferences;

    /// <inheritdoc />
    string IAnalyzerResult.ProjectFilePath => result.ProjectFilePath;

    /// <inheritdoc />
    public Guid ProjectGuid => result.ProjectGuid;

    /// <inheritdoc />
    public IEnumerable<string> ProjectReferences => result.ProjectReferences;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Properties => result.Properties;

    /// <inheritdoc />
    public string[] References => result.References;

    /// <inheritdoc />
    public string[] AnalyzerReferences => result.AnalyzerReferences;

    /// <inheritdoc />
    public string[] SourceFiles => result.SourceFiles;

    /// <inheritdoc />
    public bool Succeeded => result.Succeeded;

    /// <inheritdoc />
    public string TargetFramework => result.TargetFramework;

    /// <inheritdoc />
    public string[] PreprocessorSymbols => result.PreprocessorSymbols;

    /// <inheritdoc />
    public string[] AdditionalFiles => result.AdditionalFiles;

    /// <inheritdoc />
    public string Command => result.Command;

    /// <inheritdoc />
    public string CompilerFilePath => result.CompilerFilePath;

    /// <inheritdoc />
    public string[] CompilerArguments => result.CompilerArguments;
}

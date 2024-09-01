using Buildalyzer;
using Buildalyzer.Construction;

namespace Rocket.Surgery.Nuke.ProjectModel;

/// <summary>
/// A wrapper around the analyzer result to provide a more strongly typed model
/// </summary>
/// <param name="result"></param>
public class ProjectAnalyzerModel(IAnalyzerResult result) : IAnalyzerResult
{
    string IAnalyzerResult.GetProperty(string name)
    {
        return result.GetProperty(name);
    }

    /// <summary>
    /// The project file model
    /// </summary>
    public IProjectFile Project => result.Analyzer.ProjectFile;

    /// <summary>
    /// The package id
    /// </summary>

    public string PackageId => GetProperty<string>(nameof(PackageId)) ?? Project.Name;

    /// <summary>
    /// Is this project packable
    /// </summary>

    public bool IsPackable => GetProperty<bool>(nameof(IsPackable));

    /// <summary>
    /// Is this a test project
    /// </summary>
    public bool IsTestProject => GetProperty<bool>(nameof(IsTestProject));

    /// <summary>
    /// Get a property from the analyzer result supports a limited number of types
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public T? GetProperty<T>(string name) where T : notnull
    {
        var value = result.GetProperty(name);
        if (typeof(T) == typeof(bool)) return (T)(object)( value is "enable" or "true" );
        if (typeof(T) == typeof(string)) return (T)(object)value;
        throw new NotSupportedException(typeof(T).FullName);
    }

    /// <summary>
    /// The source project analyzer
    /// </summary>
    public ProjectAnalyzer Analyzer => result.Analyzer;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IProjectItem[]> Items => result.Items;

    AnalyzerManager IAnalyzerResult.Manager => result.Manager;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> PackageReferences => result.PackageReferences;

    /// <inheritdoc />
    public string ProjectFilePath => result.ProjectFilePath;

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

    /// <summary>
    ///   Implicitly convert the model to the project file path
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static  implicit operator string(ProjectAnalyzerModel model) => model.ProjectFilePath;
}

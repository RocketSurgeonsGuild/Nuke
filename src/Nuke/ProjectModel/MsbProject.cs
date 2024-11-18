using System.Collections.Immutable;
using System.Diagnostics;

namespace Rocket.Surgery.Nuke.ProjectModel;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public record MsbProject
(
    string Name,
    string FilePath,
    string Directory,
    ImmutableArray<MsbProperty> Properties,
    ImmutableArray<MsbItem> Items)
{
    public static MsbProject LoadProject(string projectFile)
    {
        var msbProject = NukeSolutionExtensions.ParseProject(projectFile);
        return new(
            msbProject.GetPropertyValue("AssemblyName"),
            msbProject.FullPath,
            Path.GetDirectoryName(msbProject.FullPath) ?? throw new InvalidOperationException("Could not get directory name"),
            [
                .. msbProject.AllEvaluatedProperties
                             .Select(
                                  z => new MsbProperty(
                                      z.Name,
                                      z.EvaluatedValue,
                                      z.IsGlobalProperty,
                                      z.IsReservedProperty,
                                      z.IsEnvironmentProperty,
                                      z.IsImported
                                  )
                              ),
            ],
            [
                .. msbProject.AllEvaluatedItems
                             .Select(
                                  z => new MsbItem(
                                      z.ItemType,
                                      z.EvaluatedInclude,
                                      [.. z.Metadata.Select(m => new MsbItemMetadata(m.ItemType, m.Name, m.EvaluatedValue))]
                                  )
                              ),
            ]
        );
    }

    public IEnumerable<string> TargetFrameworks => GetPropertyValues("TargetFramework", "TargetFrameworks");
    public IEnumerable<string> RuntimeIdentifiers => GetPropertyValues("RuntimeIdentifier", "RuntimeIdentifiers");

    public bool IsPackable => GetBoolProperty("IsPackable");
    public bool IsTestProject => GetBoolProperty("IsTestProject");
    public string PackageId => GetProperty("PackageId") ?? Name;

    public string? OutputType => GetProperty("OutputType");

    public ImmutableArray<MsbPackageReference> PackageReferences { get; } =
    [
        .. Items
          .Where(z => z.ItemType == "PackageReference")
          .Select(z => new MsbPackageReference(z.Include, z.Metadata)),
    ];

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    public bool GetBoolProperty(string name) => GetProperty(name) is "true" or "enable" or "enabled";

    public bool ContainsPackageReference(string packageId)
    {
        return Items.Any(z => z.ItemType == "PackageReference" && z.Include.Equals(packageId, StringComparison.OrdinalIgnoreCase));
    }

    public string? GetProperty(string name)
    {
        return Properties.FirstOrDefault(z => z.Name == name)?.Value;
    }

    public IReadOnlyCollection<string> GetPropertyValues(string name) => GetPropertyValues([name]);

    public IEnumerable<MsbItem> GetItems(string itemType)
    {
        return Items.Where(z => z.ItemType == itemType);
    }

    private IReadOnlyCollection<string> GetPropertyValues(params string[] names)
    {
        foreach (var name in names)
        {
            var property = GetProperty(name);
            if (property is { Length: > 0 })
            {
                return property.Split(';');
            }
        }

        return [];
    }
}

using System.Collections.Immutable;

namespace Rocket.Surgery.Nuke.ProjectModel;

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
            Path.GetDirectoryName(msbProject.FullPath!),
            [..msbProject.AllEvaluatedProperties
                         .Select(z => new MsbProperty(z.Name, z.EvaluatedValue, z.IsGlobalProperty, z.IsReservedProperty, z.IsEnvironmentProperty, z.IsImported))],
            [
                ..msbProject.AllEvaluatedItems
                            .Select(
                                 z => new MsbItem(
                                     z.ItemType,
                                     z.EvaluatedInclude,
                                     [..z.Metadata.Select(m => new MsbItemMetadata(m.ItemType, m.Name, m.EvaluatedValue))]
                                 )
                             )
            ]
        );
    }

    public IEnumerable<string> TargetFrameworks => GetPropertyValues("TargetFramework", "TargetFrameworks");
    public IEnumerable<string> RuntimeIdentifiers => GetPropertyValues("RuntimeIdentifier", "RuntimeIdentifiers");

    public bool IsPackable => GetProperty("Packable") == "true";
    public bool IsTestProject => GetProperty("TestProject") == "true";
    public string PackageId => GetProperty("PackageId") ?? Name;

    public string? OutputType => GetProperty("OutputType");
    public ImmutableArray<MsbPackageReference> PackageReferences { get; } = [
        ..Items.Where(z => z.ItemType == "PackageReference")
               .Select(z => new MsbPackageReference(z.Include,z.Metadata))
    ];

    public bool ContainsPackageReference(string packageId) => Items.Any(z => z.ItemType == "PackageReference" && z.Include.Equals(packageId, StringComparison.OrdinalIgnoreCase));

    public string? GetProperty(string name) => Properties.FirstOrDefault(z => z.Name == name)?.Value;
    public IReadOnlyCollection<string> GetPropertyValues(string name) => GetPropertyValues([name]);
    public IEnumerable<MsbItem> GetItems(string itemType) => Items.Where(z => z.ItemType == itemType);

    private IReadOnlyCollection<string> GetPropertyValues(params string[] names)
    {
        foreach (var name in names)
        {
            var property = GetProperty(name);
            if (property is { Length: > 0 })
                return property.Split(';');
        }

        return [];
    }
}

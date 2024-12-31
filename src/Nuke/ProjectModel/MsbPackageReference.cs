using System.Collections.Immutable;

namespace Rocket.Surgery.Nuke.ProjectModel;

public record MsbPackageReference(string Name, ImmutableArray<MsbItemMetadata> Metadata) : MsbItem("PackageReference", Name, Metadata)
{
    public string Version { get; } = Metadata.FirstOrDefault(m => m.Name == "Version")?.Value ?? "";
    public ImmutableArray<string> IncludeAssets => [.. Metadata.Where(m => m.Name == "IncludeAssets").Select(m => m.Value)];
    public ImmutableArray<string> ExcludeAssets => [.. Metadata.Where(m => m.Name == "ExcludeAssets").Select(m => m.Value)];
    public ImmutableArray<string> PrivateAssets => [.. Metadata.Where(m => m.Name == "PrivateAssets").Select(m => m.Value)];
}

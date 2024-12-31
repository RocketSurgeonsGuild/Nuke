using System.Collections.Immutable;

namespace Rocket.Surgery.Nuke.ProjectModel;

public record MsbSolution
(
    string Name,
    string FilePath,
    string Directory,
    ImmutableArray<MsbProject> Projects)
{
    public MsbProject? GetProject(string name) => Projects.FirstOrDefault(z => z.Name == name);
}

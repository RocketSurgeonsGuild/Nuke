using System.Collections.Immutable;

namespace Rocket.Surgery.Nuke.ProjectModel;

public record MsbItem(string ItemType, string Include, ImmutableArray<MsbItemMetadata> Metadata);

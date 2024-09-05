namespace Rocket.Surgery.Nuke.ProjectModel;

public record MsbProperty(string Name, string Value, bool IsGlobal, bool IsReserved, bool IsEnvironment, bool IsImported);
namespace Rocket.Surgery.Nuke;

public static class Helpers
{
    public static bool IsDotnetToolInstalled(string nugetPackageName)
    {
        var dotnetTools = Path.Combine(NukeBuild.RootDirectory, ".config/dotnet-tools.json");
        return File.Exists(dotnetTools) && File.ReadAllText(dotnetTools).Contains($"\"{nugetPackageName}\"", StringComparison.OrdinalIgnoreCase);
    }
}

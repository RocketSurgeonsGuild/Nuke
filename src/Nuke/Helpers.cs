namespace Rocket.Surgery.Nuke;

#pragma warning disable CA1724
/// <summary>
///     Helpers for Nuke builds
/// </summary>
public static class Helpers
{
    /// <summary>
    ///     Determine if a dotnet tool is installed in the dotnet-tools.json
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static bool IsDotNetToolInstalled(string nugetPackageName)
    {
        return DotNetTool.IsInstalled(nugetPackageName);
    }
}

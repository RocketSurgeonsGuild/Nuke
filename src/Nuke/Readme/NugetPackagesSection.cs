using System.Security.Cryptography;
using System.Text;
using Nuke.Common.ProjectModel;

namespace Rocket.Surgery.Nuke.Readme;

internal class NugetPackagesSection : IReadmeSection
{
    private static string NugetUrl(string packageName)
    {
        return $"https://www.nuget.org/packages/{packageName}/";
    }

    private static string NuGetDownloadsBadge(string packageName)
    {
        return $"https://img.shields.io/nuget/dt/{packageName}.svg?color=004880&logo=nuget&style=flat-square";
    }

    private static string NuGetVersionBadge(string packageName)
    {
        return $"https://img.shields.io/nuget/v/{packageName}.svg?color=004880&logo=nuget&style=flat-square";
    }

    // private static string NuGetPrereleaseVersionBadge(string packageName)
    //     => $"https://img.shields.io/nuget/vpre/{packageName}.svg?color=004880&logo=nuget&style=flat-square";

    private static string MyGetUrl(string project, string packageName)
    {
        return $"https://www.myget.org/feed/{project}/package/nuget/{packageName}";
    }

    private static string MyGetDownloadsBadge(string project, string packageName)
    {
        return $"https://img.shields.io/myget/{project}/dt/{packageName}.svg?color=004880&logo=nuget&style=flat-square";
    }

    // private static string MyGetVersionBadge(string project, string packageName)
    //     => $"https://img.shields.io/myget/{project}/v/{packageName}.svg?label=myget&color=004880&logo=nuget&style=flat-square";

    private static string MyGetPrereleaseVersionBadge(string project, string packageName)
    {
        return $"https://img.shields.io/myget/{project}/vpre/{packageName}.svg?label=myget&color=004880&logo=nuget&style=flat-square";
    }

    /// <summary>
    ///     Get the list of nuget packages with references that ensure uniqueness
    /// </summary>
    /// <param name="config"></param>
    /// <param name="references"></param>
    /// <param name="packageName"></param>
    /// <returns></returns>
    public static string GetResult(IDictionary<string, object?> config, IMarkdownReferences references, string packageName)
    {
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        using var hasher = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
#pragma warning disable CA1307
#pragma warning disable CA1308 // Normalize strings to uppercase
        var hash = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(packageName)))
                          .Replace("=", "")
                          .Substring(10)
                          .ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
#pragma warning restore CA1307
        var nugetUrlReference = references.AddReference($"nuget-{hash}", NugetUrl(packageName));
        var nugetVersionBadge = references.AddReference(
            $"nuget-version-{hash}-badge",
            NuGetVersionBadge(packageName),
            "NuGet Version"
        );
        var nugetDownloadsBadge = references.AddReference(
            $"nuget-downloads-{hash}-badge",
            NuGetDownloadsBadge(packageName),
            "NuGet Downloads"
        );
        if (!config.ContainsKey("myget"))
        {
            return $"| {packageName} | [!{nugetVersionBadge}!{nugetDownloadsBadge}]{nugetUrlReference} |";
        }

        var dcfg = config;
        var myget = dcfg["myget"] as IDictionary<object, object>;
        var mygetUrlReference = references.AddReference(
            $"myget-{hash}",
            MyGetUrl(myget!["account"].ToString()!, packageName)
        );
        var mygetVersionBadge = references.AddReference(
            $"myget-version-{hash}-badge",
            MyGetPrereleaseVersionBadge(myget["account"].ToString()!, packageName),
            "MyGet Pre-Release Version"
        );
        var mygetDownloadsBadge = references.AddReference(
            $"myget-downloads-{hash}-badge",
            MyGetDownloadsBadge(myget["account"].ToString()!, packageName),
            "MyGet Downloads"
        );
        return
            $"| {packageName} | [!{nugetVersionBadge}!{nugetDownloadsBadge}]{nugetUrlReference} | [!{mygetVersionBadge}!{mygetDownloadsBadge}]{mygetUrlReference} |";
    }

    public string Name { get; } = "nuget packages";

    public string ConfigKey { get; } = string.Empty;

    public string Process(
        IDictionary<string, object?> config,
        IMarkdownReferences references,
        IHaveSolution build
    )
    {
        var packageNames = build.Solution.WherePackable().Select(x => x.GetProperty<string>("PackageId") ?? x.Name);

        var sb = new StringBuilder();
        if (config.ContainsKey("myget"))
        {
            sb.AppendLine("| Package | NuGet | MyGet |");
            sb.AppendLine("| ------- | ----- | ----- |");
        }
        else
        {
            sb.AppendLine("| Package | NuGet |");
            sb.AppendLine("| ------- | ----- |");
        }

        foreach (var package in packageNames.OrderBy(z => z))
        {
            sb.AppendLine(GetResult(config, references, package));
        }

        return sb.ToString();
    }
}

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.ValueInjection;
using Serilog;
using static Nuke.Common.EnvironmentInfo;

#pragma warning disable CA1019
#pragma warning disable CA1813

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Injects an instance of <see cref="GitVersion" /> based on the local repository.
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[ExcludeFromCodeCoverage]
public class ComputedGitVersionAttribute : ValueInjectionAttributeBase
{
    /// <summary>
    ///     Returns if GitVersion data is available
    /// </summary>
    public static bool HasGitVer()
    {
        return Variables.Keys.Any(z => z.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase));
    }

    private readonly string? _frameworkVersion;

    /// <summary>
    ///     Computes the GitVersion for the repository.
    /// </summary>
    public ComputedGitVersionAttribute()
        : this(null)
    {
    }


    /// <summary>
    ///     Computes the GitVersion for the repository.
    /// </summary>
    /// <param name="frameworkVersion">The framework version to use with GitVersion.</param>
    public ComputedGitVersionAttribute(string? frameworkVersion)
    {
        _frameworkVersion = frameworkVersion;
    }

    /// <summary>
    ///     DisableOnUnix
    /// </summary>
    public bool DisableOnUnix { get; set; }

    /// <summary>
    ///     UpdateAssemblyInfo
    /// </summary>
    public bool UpdateAssemblyInfo { get; set; }

    /// <inheritdoc />
    public override object GetValue(MemberInfo member, object instance)
    {
        var rootDirectory = NukeBuild.RootDirectory.FindParentOrSelf(x => x.GetDirectories(".git").Any());
        if (rootDirectory == null)
        {
            Log.Warning("N git repository found, GitVersion will not be accurate");
            return new GitVersion();
        }

        var gitVersion = GetGitVersion(_frameworkVersion, UpdateAssemblyInfo);

        AzurePipelines.Instance?.UpdateBuildNumber(gitVersion.FullSemVer);
        TeamCity.Instance?.SetBuildNumber(gitVersion.FullSemVer);
        AppVeyor.Instance?.UpdateBuildVersion($"{gitVersion.FullSemVer}.build.{AppVeyor.Instance.BuildNumber}");

        return gitVersion;
    }

    internal static GitVersion GetGitVersion(string? frameworkVersion, bool updateAssemblyInfo)
    {
        if (!HasGitVer())
        {
            return GitVersionTasks.GitVersion(
                                       s => s
                                           .SetFramework(frameworkVersion)
                                           .DisableProcessLogOutput()
                                           .SetUpdateAssemblyInfo(updateAssemblyInfo)
                                           .SetProcessToolPath(
                                                NuGetToolPathResolver.GetPackageExecutable(
                                                    "GitVersion.Tool|GitVersion.CommandLine",
                                                    "gitversion.dll|gitversion.exe",
                                                    framework: frameworkVersion
                                                )
                                            )
                                   )
                                  .Result;
        }

        var json = Variables.Where(z => z.Key.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase))
                            .Aggregate(
                                 new JObject(),
                                 (acc, record) =>
                                 {
                                     var key = record.Key["GITVERSION_".Length..];
                                     acc[key] = record.Value;
                                     return acc;
                                 }
                             );
        // ReSharper disable once NullableWarningSuppressionIsUsed
        return json.ToObject<GitVersion>(
            new JsonSerializer { ContractResolver = new AllWritableContractResolver() }
        )!;
    }

    private class AllWritableContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization
        )
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Writable = true;
            return property;
        }
    }
}

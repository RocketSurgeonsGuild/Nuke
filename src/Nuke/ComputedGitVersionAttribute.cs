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

#pragma warning disable CA1019, CA1813

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Injects an instance of <see cref="GitVersion" /> based on the local repository.
/// </summary>
/// <remarks>
///     Computes the GitVersion for the repository.
/// </remarks>
/// <param name="frameworkVersion">The framework version to use with GitVersion.</param>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[ExcludeFromCodeCoverage]
public class ComputedGitVersionAttribute(string? frameworkVersion) : ValueInjectionAttributeBase
{
    /// <summary>
    ///     Computes the GitVersion for the repository.
    /// </summary>
    public ComputedGitVersionAttribute()
        : this(null) { }

    /// <inheritdoc />
    public override object GetValue(MemberInfo member, object instance)
    {
        var rootDirectory = NukeBuild.RootDirectory.FindParentOrSelf(x => x.GetDirectories(".git").Any());
        if (rootDirectory is null)
        {
            Log.Warning("No git repository found, GitVersion will not be accurate");

            return new GitVersion(
                -1,
                -1,
                -1,
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                -1,
                ""
            );
        }

        var gitVersion = GetGitVersion(_frameworkVersion, UpdateAssemblyInfo);

        AzurePipelines.Instance?.UpdateBuildNumber(gitVersion.FullSemVer);
        TeamCity.Instance?.SetBuildNumber(gitVersion.FullSemVer);
        AppVeyor.Instance?.UpdateBuildVersion($"{gitVersion.FullSemVer}.build.{AppVeyor.Instance.BuildNumber}");

        return gitVersion;
    }

    /// <summary>
    ///     Returns if GitVersion data is available
    /// </summary>
    public static bool HasGitVer() => Variables.Keys.Any(z => z.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     DisableOnUnix
    /// </summary>
    public bool DisableOnUnix { get; set; }

    /// <summary>
    ///     UpdateAssemblyInfo
    /// </summary>
    public bool UpdateAssemblyInfo { get; set; }

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
                                                    "GitVersion.Tool",
                                                    "gitversion.dll|gitversion.exe",
                                                    framework: frameworkVersion
                                                )
                                            )
                                   )
                                  .Result;
        }

        var json = Variables
                  .Where(z => z.Key.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase))
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
            new() { ContractResolver = new AllWritableContractResolver() }
        )!;
    }

    private readonly string? _frameworkVersion = frameworkVersion;
}

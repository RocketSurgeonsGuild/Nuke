using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.EnvironmentInfo;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Injects an instance of <see cref="GitVersion" /> based on the local repository.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    [ExcludeFromCodeCoverage]
    public class ComputedGitVersionAttribute : InjectionAttributeBase
    {
        /// <summary>
        /// Returns if GitVersion data is available
        /// </summary>
        public static bool HasGitVer()
            => Variables.Keys.Any(z => z.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase));

        private readonly string _frameworkVersion;

        /// <summary>
        /// Computes the GitVersion for the repository.
        /// </summary>
        public ComputedGitVersionAttribute()
            : this("netcoreapp3.1") { }


        /// <summary>
        /// Computes the GitVersion for the repository.
        /// </summary>
        /// <param name="frameworkVersion">The framework version to use with GitVersion.</param>
        public ComputedGitVersionAttribute(string frameworkVersion) => _frameworkVersion = frameworkVersion;


        /// <summary>
        /// DisableOnUnix
        /// </summary>
        public bool DisableOnUnix { get; set; }

        /// <summary>
        /// UpdateAssemblyInfo
        /// </summary>
        public bool UpdateAssemblyInfo { get; set; }

        /// <inheritdoc />
        public override object? GetValue(MemberInfo member, object instance)
        {
            var rootDirectory = FileSystemTasks.FindParentDirectory(
                NukeBuild.RootDirectory,
                x => x.GetDirectories(".git").Any()
            );
            if (rootDirectory == null)
            {
                Logger.Warn("No git repository found, GitVersion will not be accurate.");
                return new GitVersion();
            }

            if (!HasGitVer())
            {
                return GitVersionTasks.GitVersion(
                        s => s
                           .SetFramework(_frameworkVersion)
                           .DisableLogOutput()
                           .SetUpdateAssemblyInfo(UpdateAssemblyInfo)
                    )
                   .Result;
            }

            var json = Variables.Where(z => z.Key.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase))
               .Aggregate(
                    new JObject(),
                    (acc, record) =>
                    {
                        var key = record.Key.Substring("GITVERSION_".Length);
                        acc[key] = record.Value;
                        return acc;
                    }
                );
            return json.ToObject<GitVersion>(
                new JsonSerializer { ContractResolver = new AllWritableContractResolver() }
            );
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
}
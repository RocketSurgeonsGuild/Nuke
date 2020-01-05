using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nuke.Common.Tools;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.EnvironmentInfo;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Injects an instance of <see cref="GitVersion" /> based on the local repository.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    public class ComputedGitVersionAttribute : InjectionAttributeBase
    {
        /// <summary>
        /// Returns if GitVersion data is available
        /// </summary>
        public static bool HasGitVer()
            => Variables.Keys.Any(z => z.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase));

        /// <inheritdoc />
        public override object GetValue(MemberInfo member, object instance)
        {
            if (HasGitVer())
            {
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

            return GitVersionTasks.GitVersion(s => s
                    .SetFramework("netcoreapp3.1")
                    .DisableLogOutput()
                    .SetUpdateAssemblyInfo(UpdateAssemblyInfo))
                .Result;
        }


        /// <summary>
        /// </summary>
        public bool DisableOnUnix { get; set; }

        /// <summary>
        /// </summary>
        public bool UpdateAssemblyInfo { get; set; }

        private class AllWritableContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(
                [NotNull] MemberInfo member,
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
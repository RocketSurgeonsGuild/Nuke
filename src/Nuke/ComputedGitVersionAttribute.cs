using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.EnvironmentInfo;
using Nuke.Common.Tooling;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using JetBrains.Annotations;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Injects an instance of <see cref="GitVersion"/> based on the local repository.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    internal class ComputedGitVersionAttribute : GitVersionAttribute
    {
        /// <inheritdoc />
        public override object GetValue(MemberInfo member, object instance)
        {
            if (HasGitVer())
            {
                var json = Variables.Where(z => z.Key.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase)).Aggregate(new JObject(), (acc, record) =>
                {
                    var key = record.Key.Substring("GITVERSION_".Length);
                    acc[key] = record.Value;
                    return acc;
                });
                return json.ToObject<GitVersion>(new JsonSerializer() { ContractResolver = new AllWritableContractResolver() });
            }
            return base.GetValue(member, instance);
        }

        /// <summary>
        /// Returns if GitVersion data is available
        /// </summary>
        public static bool HasGitVer()
        {
            return Variables.Keys.Any(z => z.StartsWith("GITVERSION_", StringComparison.OrdinalIgnoreCase));
        }

        private class AllWritableContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty([NotNull] MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.Writable = true;
                return property;
            }
        }
    }

}

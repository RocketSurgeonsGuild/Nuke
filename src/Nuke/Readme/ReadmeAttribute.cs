using System.Diagnostics;
using System.Reflection;
using Nuke.Common.ProjectModel;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;

namespace Rocket.Surgery.Nuke.Readme
{

    /// <summary>
    /// Injects an instance of <see cref="ReadmeUpdater"/> based on the local repository.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    internal class ReadmeAttribute : InjectionAttributeBase
    {
        /// <inheritdoc />
        public override object GetValue(MemberInfo member, object instance)
        {
            return new ReadmeUpdater();
        }
    }
}

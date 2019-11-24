using System.Reflection;
using JetBrains.Annotations;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.Readme
{
    /// <summary>
    /// Injects an instance of <see cref="ReadmeUpdater" /> based on the local repository.
    /// </summary>
    [PublicAPI]
    [UsedImplicitly(ImplicitUseKindFlags.Default)]
    internal class ReadmeAttribute : InjectionAttributeBase
    {
        /// <inheritdoc />
        public override object GetValue(MemberInfo member, object instance) => new ReadmeUpdater();
    }
}
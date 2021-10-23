using System.Reflection;
using Nuke.Common.ValueInjection;

namespace Rocket.Surgery.Nuke.Readme;

/// <summary>
///     Injects an instance of <see cref="ReadmeUpdater" /> based on the local repository.
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
internal sealed class ReadmeAttribute : ValueInjectionAttributeBase
{
    /// <inheritdoc />
    public override object GetValue(MemberInfo member, object instance)
    {
        return new ReadmeUpdater();
    }
}

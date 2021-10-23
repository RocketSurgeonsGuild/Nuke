using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Nuke.Common.Git;
using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <inheritdoc />
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[ExcludeFromCodeCoverage]
public sealed class OptionalGitRepositoryAttribute : GitRepositoryAttribute
{
    /// <inheritdoc />
    public override object? GetValue(MemberInfo member, object instance)
    {
        var rootDirectory = FileSystemTasks.FindParentDirectory(
            NukeBuild.RootDirectory,
            x => x.GetDirectories(".git").Any()
        );
        if (rootDirectory != null)
        {
            return base.GetValue(member, instance);
        }

        Logger.Warn("No git repository found, GitRepository will not be available.");
        return null;
    }
}

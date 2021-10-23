using NuGet.Configuration;
using Nuke.Common.Execution;

#pragma warning disable CA1813
#pragma warning disable CA2201
namespace Rocket.Surgery.Nuke;

/// <summary>
///     Ensures that the package source name has credentials set
///     This is useful to ensure that credentials are defined on a users local environment
/// </summary>
[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Default)]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class EnsurePackageSourceHasCredentialsAttribute : BuildExtensionAttributeBase, IOnBuildCreated
{
    /// <summary>
    ///     Ensures that the package source name has credentials set
    ///     This is useful to ensure that credentials are defined on a users local environment
    /// </summary>
    public EnsurePackageSourceHasCredentialsAttribute(string sourceName)
    {
        SourceName = sourceName;
    }

    /// <summary>
    ///     The nuget source name
    /// </summary>
    public string SourceName { get; }

    /// <inheritdoc />
    public void OnBuildCreated(NukeBuild build, IReadOnlyCollection<ExecutableTarget> executableTargets)
    {
        var settings = Settings.LoadDefaultSettings(NukeBuild.RootDirectory);
        var packageSourceProvider = new PackageSourceProvider(settings);

        var source = packageSourceProvider.LoadPackageSources()
                                          .FirstOrDefault(x => x.Name.Equals(SourceName, StringComparison.OrdinalIgnoreCase));
        if (source == null)
        {
            var error =
                $"NuGet Package Source {SourceName} could not be found. This is required for the build to complete.";
            Logger.Error(error);
            throw new Exception(error);
        }

        if (source.Credentials?.IsValid() != true)
        {
            var error =
                $"NuGet Package Source {SourceName} does not have any credentials defined.  Please configure the credentials for {SourceName} to build.";
            Logger.Error(error);
            throw new Exception(error);
        }
    }
}

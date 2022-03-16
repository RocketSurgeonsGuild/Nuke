using Nuke.Common.IO;

namespace Rocket.Surgery.Nuke;

/// <summary>
/// Defines a common property for code signing.
/// </summary>
public interface IHaveSignCertificate : IHave
{
    /// <summary>
    ///     The path to the signing certificate.
    /// </summary>
    [Parameter("The directory where certificate exists", Name = "Certificate")]
    public AbsolutePath SigningCertificateFilePath => EnvironmentInfo.GetVariable<AbsolutePath>("Certificate")
                                        ?? TryGetValue(() => SigningCertificateFilePath)
                                        ?? NukeBuild.RootDirectory / "signing.pfx";
}

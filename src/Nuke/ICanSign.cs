using Nuke.Common.Tools.SignTool;
using static Nuke.Common.Tools.SignTool.SignToolTasks;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Sign with SignTool
/// </summary>
public interface ICanSign : ICan, IHaveSignCertificate, IHaveArtifacts
{
    /// <summary>
    ///     sign all binaries in the all artifact directories
    /// </summary>
    public Target Sign => _ => _.Executes(() =>
    {
        // https://makolyte.com/dotnet-how-to-sign-your-code-with-a-code-signing-certificate/
        SignTool(configurator => configurator.SetFile(SigningCertificateFilePath).SetFiles(ArtifactsDirectory));
    });
}

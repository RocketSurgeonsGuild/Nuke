namespace Rocket.Surgery.Nuke;

[PublicAPI]
public interface IArguments
{
    string FilterSecrets(string text);
    string RenderForExecution();
    string RenderForOutput();
}

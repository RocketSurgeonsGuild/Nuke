using Nuke.Common;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.IO.PathConstruction;
using Nuke.Common.Tooling;
using Newtonsoft.Json.Linq;

namespace Rocket.Surgery.Nuke
{
    static class CustomDotNetBuildExtensions
    {
        public static T SetBinaryLogger<T>(this T settings, AbsolutePath path, MSBuildBinaryLogImports imports) where T : ToolSettings
        {
            var existingArgs = settings.ArgumentConfigurator;
            return settings.SetArgumentConfigurator(args =>
                existingArgs(args).Add($"/bl:{path};ProjectImports={imports}"));
        }
        public static T SetFileLogger<T>(this T settings, AbsolutePath path, Verbosity verbosity) where T : ToolSettings
        {
            var existingArgs = settings.ArgumentConfigurator;
            return settings.SetArgumentConfigurator(args =>
                existingArgs(args).Add($"/fileLogger /fileloggerparameters:ShowTimestamp;Verbosity={verbosity};LogFile=\"{path}\""));
        }

        public static T SetGitVersionEnvironment<T>(this T settings, GitVersion gitVersion) where T : ToolSettings
        {
            foreach (var item in JObject.FromObject(gitVersion))
            {
                settings = settings.AddEnvironmentVariable($"gitversion_{item.Key}".ToUpperInvariant(), item.Value.ToString());
            }
            return settings;
        }
    }

}

using System.Collections.Immutable;
using System.Text.Json;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke;

#pragma warning disable CA1724
/// <summary>
///     DotnetTool for Nuke builds
/// </summary>
public static class DotnetTool
{
    private static ResolvedToolsManifest? _toolsManifest;
    private static Lazy<AbsolutePath> ToolsManifestLocation { get; } = new(() => NukeBuild.RootDirectory / ".config/dotnet-tools.json");

    /// <summary>
    ///     Determine if a dotnet tool is installed in the dotnet-tools.json
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static bool IsInstalled(string nugetPackageName) => ResolveToolsManifest().IsInstalled(nugetPackageName);

    /// <summary>
    /// Gets the tool definition for a given local dotnet tool
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static Tool GetTool(string nugetPackageName)
    {
        return ResolveToolsManifest().GetTool(nugetPackageName);
    }

    private static ResolvedToolsManifest ResolveToolsManifest()
    {
        if (_toolsManifest is null)
        {
            if (ToolsManifestLocation.Value.FileExists())
            {
                _toolsManifest = ResolvedToolsManifest.Create(
                    JsonSerializer.Deserialize<ToolsManifset>(
                        File.ReadAllText(ToolsManifestLocation.Value), new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        }
                    )
                );
            }
            else
            {
                _toolsManifest = new ResolvedToolsManifest(ImmutableDictionary<string, FullToolCommandDefinition>.Empty);
            }
        }

        return _toolsManifest!;
    }

    internal record FullToolCommandDefinition(
        string PackageId,
        string Version,
        string Command
    );

    internal record ResolvedToolsManifest(ImmutableDictionary<string, FullToolCommandDefinition> CommandDefinitions)
    {
        public static ResolvedToolsManifest Create(ToolsManifset source)
        {
            var commandBuilder = ImmutableDictionary<string, FullToolCommandDefinition>.Empty.ToBuilder();
            commandBuilder.KeyComparer = StringComparer.OrdinalIgnoreCase;
            foreach (var tool in source.Tools)
            {
                Console.WriteLine("TOOL: " + tool.Key);
                foreach (var command in tool.Value.Commands)
                {
                    commandBuilder.Add(command, new FullToolCommandDefinition(tool.Key, tool.Value.Version, command));
                }
            }

            return new ResolvedToolsManifest(commandBuilder.ToImmutable());
        }

        public bool IsInstalled(string commandName) => CommandDefinitions.ContainsKey(commandName) || CommandDefinitions.Values.Any(z => z.PackageId.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        public Tool GetTool(string nugetPackageName) => CommandDefinitions.TryGetValue(nugetPackageName, out var tool)
            ? (arguments, directory, variables, timeout, output, invocation, logger, handler) => DotNetTasks.DotNet($"{tool.Command} {arguments.ToStringAndClear()}", directory, variables, timeout, output, invocation, logger, handler)
            : throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");
    }

    internal class ToolsManifset
    {
        public Dictionary<string, ToolDefinition> Tools { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal class ToolDefinition
    {
        public string Version { get; set; }
        public string[] Commands { get; set; }
    }
}

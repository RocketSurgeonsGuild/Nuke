using System.Collections.Immutable;
using System.Text.Json;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace Rocket.Surgery.Nuke;

#pragma warning disable CA1724
/// <summary>
///     DotNetTool for Nuke builds
/// </summary>
public static class DotNetTool
{
    private static ResolvedToolsManifest? _toolsManifest;
    private static Lazy<AbsolutePath> ToolsManifestLocation { get; } = new(() => NukeBuild.RootDirectory / ".config/dotnet-tools.json");

    /// <summary>
    ///     Determine if a dotnet tool is installed in the dotnet-tools.json
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static bool IsInstalled(string nugetPackageName)
    {
        return ResolveToolsManifest().IsInstalled(nugetPackageName);
    }

    /// <summary>
    ///     Gets the tool definition for a given local dotnet tool
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static Tool GetTool(string nugetPackageName)
    {
        return ResolveToolsManifest().GetTool(nugetPackageName);
    }

    private static ResolvedToolsManifest ResolveToolsManifest()
    {
        if (_toolsManifest is not null) return _toolsManifest;
        if (ToolsManifestLocation.Value.FileExists())
        {
#pragma warning disable CA1869
            _toolsManifest = ResolvedToolsManifest.Create(
                // ReSharper disable once NullableWarningSuppressionIsUsed
                JsonSerializer.Deserialize<ToolsManifset>(
                    File.ReadAllText(ToolsManifestLocation.Value), new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    }
                )!
            );
#pragma warning restore CA1869
        }
        else
        {
            _toolsManifest = new ResolvedToolsManifest(ImmutableDictionary<string, FullToolCommandDefinition>.Empty);
        }

        return _toolsManifest;
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
                foreach (var command in tool.Value.Commands)
                {
                    commandBuilder.Add(command, new FullToolCommandDefinition(tool.Key, tool.Value.Version, command));
                }
            }

            return new ResolvedToolsManifest(commandBuilder.ToImmutable());
        }

        public bool IsInstalled(string commandName)
        {
            return CommandDefinitions.ContainsKey(commandName)
                || CommandDefinitions.Values.Any(z => z.PackageId.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        }

        public Tool GetTool(string nugetPackageName)
        {
            return CommandDefinitions.TryGetValue(nugetPackageName, out var tool)
                ? (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
                {
                    var args = arguments.ToStringAndClear();
                    args = args.StartsWith('"')
                        ? string.Concat("\"", tool.Command, " ", args.AsSpan(1))
                        : string.Concat(tool.Command, " ", args);
                    return DotNetTasks.DotNet(args, directory, variables, timeout, output, invocation, logger, handler);
                }
            : throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");
        }
    }

    internal class ToolsManifset
    {
        public Dictionary<string, ToolDefinition> Tools { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    internal class ToolDefinition
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        public string Version { get; set; } = null!;
        public string[] Commands { get; set; } = Array.Empty<string>();
    }
}

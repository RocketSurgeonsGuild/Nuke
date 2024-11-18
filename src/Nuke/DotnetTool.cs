using System.Collections.Immutable;
using System.Text.Json;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Rocket.Surgery.Nuke;

#pragma warning disable CA1724
/// <summary>
///     DotNetTool for Nuke builds
/// </summary>
public static class DotNetTool
{
    /// <summary>
    ///     Determine if a dotnet tool is installed in the dotnet-tools.json
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static bool IsInstalled(string nugetPackageName) => ResolveToolsManifest().IsInstalled(nugetPackageName);

    /// <summary>
    ///     Gets the tool definition for a given local dotnet tool
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static Tool GetTool(string nugetPackageName) => ResolveToolsManifest().GetTool(nugetPackageName);

    /// <summary>
    ///     Gets the tool definition for a given local dotnet tool
    /// </summary>
    /// <param name="nugetPackageName"></param>
    /// <returns></returns>
    public static ProperTool GetProperTool(string nugetPackageName) => ResolveToolsManifest().GetProperTool(nugetPackageName);

    private static ResolvedToolsManifest? toolsManifest;
    private static Lazy<AbsolutePath> ToolsManifestLocation { get; } = new(() => NukeBuild.RootDirectory / ".config/dotnet-tools.json");

    private static ResolvedToolsManifest ResolveToolsManifest()
    {
        if (toolsManifest is { }) return toolsManifest;
        if (ToolsManifestLocation.Value.FileExists())
        {
            #pragma warning disable CA1869
            toolsManifest = ResolvedToolsManifest.Create(
                // ReSharper disable once NullableWarningSuppressionIsUsed
                JsonSerializer.Deserialize<ToolsManifset>(
                    File.ReadAllText(ToolsManifestLocation.Value),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                )!
            );
            #pragma warning restore CA1869
        }
        else
        {
            toolsManifest = new(ImmutableDictionary<string, FullToolCommandDefinition>.Empty);
        }

        return toolsManifest;
    }
}

internal record FullToolCommandDefinition
(
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
                commandBuilder.Add(command, new(tool.Key, tool.Value.Version, command));
            }
        }

        return new(commandBuilder.ToImmutable());
    }

    private static void DefaultLogger(OutputType kind, string message)
    {
        // ReSharper disable TemplateIsNotCompileTimeConstantProblem
        if (kind == OutputType.Std)
            Log.Information(message);
        else
            Log.Warning(message);
        // ReSharper restore TemplateIsNotCompileTimeConstantProblem
    }

    public bool IsInstalled(string commandName)
    {
        return CommandDefinitions.ContainsKey(commandName)
         || CommandDefinitions.Values.Any(z => z.PackageId.Equals(commandName, StringComparison.OrdinalIgnoreCase));
    }

    public Tool GetTool(string nugetPackageName)
    {
        if (CommandDefinitions.TryGetValue(nugetPackageName, out var tool))
        {
            return (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
                   {
                       var newArgs = new ArgumentStringHandler();
                       newArgs.AppendLiteral(tool.Command);
                       newArgs.AppendLiteral(" ");
                       newArgs.AppendLiteral(arguments.ToStringAndClear().TrimMatchingDoubleQuotes());
                       arguments.AppendLiteral(newArgs.ToStringAndClear());
                       return DotNetTasks.DotNet(
                           arguments,
                           directory,
                           variables,
                           timeout,
                           output,
                           invocation,
                           // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                           logger ?? DefaultLogger,
                           handler
                       );
                   };
        }
        throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");
    }

    public ProperTool GetProperTool(string nugetPackageName)
    {
        return CommandDefinitions.TryGetValue(nugetPackageName, out var tool)
            ? (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
              {
                  var process = ProcessTasks.StartProcess(
                      DotNetTasks.DotNetPath,
                      string.Concat(tool.Command, " ", arguments.RenderForExecution()),
                      directory,
                      variables,
                      timeout,
                      output,
                      invocation,
                      // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                      logger ?? DefaultLogger
                  );
                  ( handler ?? ( p => ProcessTasks.DefaultExitHandler(null, p) ) ).Invoke(process.AssertWaitForExit());
                  return process.Output;
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
    public string Version { get; set; } = null!;
    public string[] Commands { get; set; } = [];
    // ReSharper disable once NullableWarningSuppressionIsUsed
}

/// <summary>
///     Copy of tool just for stirngs to allow going from arguments
/// </summary>
public delegate IReadOnlyCollection<Output> ProperTool(
    Arguments arguments,
    string? workingDirectory = null,
    IReadOnlyDictionary<string, string>? environmentVariables = null,
    int? timeout = null,
    bool? logOutput = null,
    bool? logInvocation = null,
    Action<OutputType, string>? logger = null,
    Action<IProcess>? exitHandler = null
);

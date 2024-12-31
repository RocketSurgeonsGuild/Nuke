using System.Collections.Immutable;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Rocket.Surgery.Nuke;

internal class ResolvedToolsManifest
(
    ImmutableDictionary<string, ToolDefinition> toolDefinitions,
    ImmutableDictionary<string, FullToolCommandDefinition> commandDefinitions
)
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

        return new(source.Tools.ToImmutableDictionary(z => z.Key, z => z.Value, StringComparer.OrdinalIgnoreCase), commandBuilder.ToImmutable());
    }

    private static void DefaultLogger(OutputType kind, string message)
    {
        // ReSharper disable TemplateIsNotCompileTimeConstantProblem
        if (kind == OutputType.Std)
        {
            Log.Information(message);
        }
        else
        {
            Log.Warning(message);
        }
        // ReSharper restore TemplateIsNotCompileTimeConstantProblem
    }

    private static Tool CreateHandler(string command) => (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
                                                         {
                                                             var newArgs = new ArgumentStringHandler();
                                                             newArgs.AppendLiteral(command);
                                                             newArgs.AppendLiteral(" ");
                                                             newArgs.AppendLiteral(arguments.ToStringAndClear().TrimMatchingDoubleQuotes());
                                                             arguments.AppendLiteral(
                                                                 newArgs.ToStringAndClear().TrimMatchingDoubleQuotes().Replace("\\\"", "\"")
                                                             );
                                                             return DotNetTasks.DotNet(
                                                                 arguments,
                                                                 directory,
                                                                 variables,
                                                                 timeout,
                                                                 output,
                                                                 invocation,
                                                                 // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                                                 logger ?? DefaultLogger,
                                                                 process =>
                                                                 {
                                                                     if (handler is null)
                                                                     {
                                                                         return process.AssertZeroExitCode();
                                                                     }

                                                                     handler.Invoke(process);
                                                                     return process;
                                                                 }
                                                             );
                                                         };

    public bool IsInstalled(string commandName) => commandDefinitions.ContainsKey(commandName) || toolDefinitions.ContainsKey(commandName);

    public ToolDefinition GetToolDefinition(string nugetPackageName) => toolDefinitions.TryGetValue(nugetPackageName, out var tool)
        ? tool
        : throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");

    public Tool GetTool(string nugetPackageName) => toolDefinitions.TryGetValue(nugetPackageName, out var tool)
        ? CreateHandler(tool.Commands.First())
        : commandDefinitions.TryGetValue(nugetPackageName, out var command)
            ? CreateHandler(command.Command)
            : throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");

    public Tool GetProperTool(string nugetPackageName) => GetTool(nugetPackageName);
}

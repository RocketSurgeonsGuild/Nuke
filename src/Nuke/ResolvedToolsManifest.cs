using System.Collections.Immutable;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace Rocket.Surgery.Nuke;

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
                           process =>
                           {
                               handler?.Invoke(process);
                               return null;
                           }
                       );
                   };
        }

        throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");
    }

    public Tool GetProperTool(string nugetPackageName)
    {
        return CommandDefinitions.TryGetValue(nugetPackageName, out var tool)
            ? (arguments, directory, variables, timeout, output, invocation, logger, handler) =>
              {
                  var newArgs = new ArgumentStringHandler();
                  newArgs.AppendLiteral(tool.Command);
                  newArgs.AppendLiteral(" ");
                  newArgs.AppendLiteral(arguments.ToStringAndClear().TrimMatchingDoubleQuotes());
                  arguments.AppendLiteral(newArgs.ToStringAndClear());

                  var process = ProcessTasks.StartProcess(
                      DotNetTasks.DotNetPath,
                      newArgs,
                      directory,
                      variables,
                      timeout,
                      output,
                      invocation,
                      // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                      logger ?? DefaultLogger
                  );
                  ( handler ?? ( p => process.AssertZeroExitCode() ) ).Invoke(process.AssertWaitForExit());
                  return process.Output;
              }
            : throw new InvalidOperationException($"Tool {nugetPackageName} is not installed");
    }
}
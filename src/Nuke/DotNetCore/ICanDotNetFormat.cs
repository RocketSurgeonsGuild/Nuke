using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Serilog;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines the targets and properties for using DotNet Format
/// </summary>
public interface ICanDotNetFormat : IHaveSolution, ICanLint, IHaveOutputLogs
{
    /// <summary>
    ///     The default severity to use for DotNetFormat
    /// </summary>
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.warn;

    /// <summary>
    ///     The default profile to use for JetBrainsCleanupCode
    /// </summary>
    public string JetBrainsCleanupCodeProfile => "Full Cleanup";

    /// <summary>
    /// A list of diagnostic ids to exclude from the dotnet format
    /// </summary>
    public ImmutableArray<string> DotNetFormatExcludedDiagnostics => [];

    /// <summary>
    ///   A list of diagnostic ids to include in the dotnet format
    /// </summary>
    public ImmutableArray<string>? DotNetFormatIncludedDiagnostics => null;

    /// <summary>
    ///     The dotnet format target
    /// </summary>
    public Target DotNetFormat => d =>
                                      d
                                         .TriggeredBy(Lint)
                                         .DependsOn(ResolveLintPaths)
                                         .Before(PostLint)
                                         .OnlyWhenDynamic(() => IsLocalBuild || LintPaths.HasPaths)
                                         .Executes(
                                              () =>
                                              {
                                                  var arguments = new Arguments()
                                                                 .Add("format")
                                                                 .Add("--severity {value}", DotNetFormatSeverity.ToString())
                                                                 .Add("--verbosity {value}", Verbosity.MapVerbosity(MSBuildVerbosity.Normal).ToString().ToLowerInvariant())
                                                                 .Add("--no-restore");

                                                  if (DotNetFormatIncludedDiagnostics is { Length: > 1 })
                                                      arguments.Add("--diagnostics {value}", string.Join(" ", DotNetFormatIncludedDiagnostics.Value));

                                                  if (DotNetFormatExcludedDiagnostics is { Length: > 1 })
                                                      arguments.Add("--exclude-diagnostics {value}", string.Join(" ", DotNetFormatExcludedDiagnostics));

                                                  arguments.Add("--binarylog {value}", LogsDirectory / "dotnet-format.binlog");

                                                  if (LintPaths.HasPaths)
                                                  {
                                                      var matcher = new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude("**/*.cs");
                                                      arguments.Add("--include {value}", string.Join(",", LintPaths.Glob(matcher)));
                                                  }

                                                  return DotNetTasks.DotNet(
                                                      arguments.RenderForExecution(),
                                                      RootDirectory,
                                                      logInvocation: false,
                                                      logger: (_, s) => Log.Logger.Information(s)
                                                  );
                                              }
                                          );

    /// <summary>
    ///     Use the jetbrains code cleanup tool to format the code if installed
    /// </summary>
    public Target JetBrainsCleanupCode => d =>
                                              d
                                                 .TriggeredBy(Lint)
                                                 .DependsOn(ResolveLintPaths)
                                                 .After(DotNetFormat)
                                                 .Before(PostLint)
                                                 .OnlyWhenDynamic(() => IsLocalBuild || LintPaths.HasPaths)
                                                 .OnlyWhenStatic(() => DotnetTool.IsInstalled("jb"))
                                                 .Executes(
                                                      () =>
                                                      {
                                                          var arguments = new Arguments()
                                                                         .Add("cleanupcode")
                                                                         .Add(Solution.Path)
                                                                         .Add("--profile={value}", JetBrainsCleanupCodeProfile)
                                                                         .Add(
                                                                              "--disable-settings-layers={value}",
                                                                              "GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
                                                                          );
                                                          if (LintPaths.HasPaths)
                                                          {
                                                              var matcher = new Matcher(StringComparison.OrdinalIgnoreCase)
                                                                           .AddInclude("**/*.cs")
                                                                           .AddInclude("**/*.csproj")
                                                                           .AddInclude("**/*.targets")
                                                                           .AddInclude("**/*.props");
                                                              arguments.Add("--include={value}", string.Join(";", LintPaths.Glob(matcher)));
                                                          }

                                                          return DotnetTool.GetProperTool("jb")(arguments, RootDirectory/*, logInvocation: false*/);
                                                      }
                                                  );
}

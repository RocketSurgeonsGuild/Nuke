using System.Collections.Immutable;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.ReSharper;
using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines the targets and properties for using DotNet Format
/// </summary>
[PublicAPI]
public interface ICanDotNetFormat : IHaveSolution, ICanLint, IHaveOutputLogs
{
    private static Matcher? jbMatcher;
    private static Matcher? dnfMatcher;

    /// <summary>
    ///     The default severity to use for DotNetFormat
    /// </summary>
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.info;

    /// <summary>
    ///     The default profile to use for JetBrainsCleanupCode
    /// </summary>
    public string JetBrainsCleanupCodeProfile => "Full Cleanup";

    /// <summary>
    ///     A list of diagnostic ids to exclude from the dotnet format
    /// </summary>
    public ImmutableArray<string> DotNetFormatExcludedDiagnostics =>
    [
        "RCS1060",
        "RCS1110",
        "RCS1250",
        "RCS1163",
        "CS1591",
        "CS0108",
        "CS0246",
        "IDE1006",
        "RCS1175",
        "IDE0052",
        "RCS1246",
        "RCS1112",
    ];

    /// <summary>
    ///     A list of diagnostic ids to include in the dotnet format
    /// </summary>
    public ImmutableArray<string>? DotNetFormatIncludedDiagnostics => null;

    /// <summary>
    ///     The dotnet format target
    /// </summary>
    public Target DotnetFormat => d =>
                                      d
                                         .TriggeredBy(Lint)
                                         .Before(PostLint)
                                         .OnlyWhenDynamic(() => LintPaths.IsLocalLintOrMatches(DotnetFormatMatcher))
                                         .Net9MsBuildFix()
                                         .Executes(
                                              () =>
                                              {

                                                  var formatSettings = new DotNetFormatSettings()
                                                                      .SetSeverity(DotNetFormatSeverity)
                                                                      .SetVerbosity(Verbosity.MapVerbosity(DotNetVerbosity.normal))
                                                                      .EnableNoRestore()
                                                                      .SetBinaryLog(LogsDirectory / "dotnet-format.binlog")

                                                                       ;

                                                  if (DotNetFormatIncludedDiagnostics is { Length: > 0 })
                                                  {
                                                      formatSettings = formatSettings.SetProcessAdditionalArguments(["--diagnostics", .. DotNetFormatIncludedDiagnostics.Value]);
                                                  }

                                                  if (DotNetFormatExcludedDiagnostics is { Length: > 0 })
                                                  {
                                                      formatSettings = formatSettings.SetProcessAdditionalArguments(["--exclude-diagnostics", .. DotNetFormatExcludedDiagnostics]);
                                                  }

                                                  if (LintPaths.Glob(DotnetFormatMatcher) is { Count: > 0 } values)
                                                  {
                                                      formatSettings = formatSettings.AddInclude(values.Select(z => z.ToString()));
                                                  }
                                                  else if (LintPaths.AllPaths.Glob(DotnetFormatMatcher) is { Count: > 0 } allFiles)
                                                  {
                                                      formatSettings = formatSettings.AddInclude(allFiles.Select(z => z.ToString()));
                                                  }

                                                  _ = DotNetTasks.DotNetFormat(formatSettings);
                                              }
                                          );

    /// <summary>
    ///     Use the jetbrains code cleanup tool to format the code if installed
    /// </summary>
    public Target JetBrainsCleanupCode => d =>
                                              d
                                                 .TriggeredBy(Lint)
                                                 .After(DotnetFormat)
                                                 .Before(PostLint)
                                                 .OnlyWhenStatic(() => DotNetTool.IsInstalled("jb"))
                                                 // disable for local stagged runs, as it takes a long time.
                                                 .OnlyWhenStatic(
                                                      () => ( IsLocalBuild && LintPaths.Trigger != LintTrigger.Staged )
                                                       || !IsLocalBuild
                                                       || ExecutionPlan.Contains(JetBrainsCleanupCode)
                                                  )
                                                 .OnlyWhenDynamic(() => LintPaths.IsLocalLintOrMatches(JetBrainsCleanupCodeMatcher))
                                                 .Net9MsBuildFix()
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
                                                          if (LintPaths.Glob(JetBrainsCleanupCodeMatcher) is { Count: > 0 } files)
                                                          {
                                                              _ = arguments.Add("--include={value}", files, ';');
                                                          }
                                                          else if (LintPaths.AllPaths.Glob(JetBrainsCleanupCodeMatcher) is { Count: > 0 } allFiles)
                                                          {
                                                              _ = arguments.Add("--include={value}", allFiles, ';');
                                                          }

                                                          _ = DotNetTool.GetTool("jb")(
                                                              arguments.RenderForExecution(),
                                                              RootDirectory,
                                                              logOutput: true,
                                                              logInvocation: Verbosity == Verbosity.Verbose,
                                                              // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                                              logger: static (t, s) => Log.Write(
                                                                          ( t == OutputType.Err ) ? LogEventLevel.Error : LogEventLevel.Information,
                                                                          s
                                                                      )
                                                          );
                                                      }
                                                  );

    /// <summary>
    ///     The default matcher for jetbrains cleanup code
    /// </summary>
    public Matcher JetBrainsCleanupCodeMatcher => jbMatcher ??= new Matcher(StringComparison.OrdinalIgnoreCase)
                                                               .AddInclude("**/*.cs")
                                                               .AddInclude("**/*.csproj")
                                                               .AddInclude("**/*.targets")
                                                               .AddInclude("**/*.props")
                                                               .AddInclude("**/*.xml");

    /// <summary>
    ///     The default matcher for jetbrains cleanup code
    /// </summary>
    public Matcher DotnetFormatMatcher => dnfMatcher ??= new Matcher(StringComparison.OrdinalIgnoreCase)
       .AddInclude("**/*.cs");
}

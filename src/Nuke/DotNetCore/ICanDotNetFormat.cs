using Microsoft.Extensions.FileSystemGlobbing;

using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Nuke.DotNetCore;

/// <summary>
///     Defines the targets and properties for using DotNet Format
/// </summary>
[PublicAPI]
public interface ICanDotNetFormat : IHaveSolution, ICanLint, IHaveOutputLogs
{
    /// <summary>
    ///     The dotnet format target
    /// </summary>
    public Target DotnetFormat =>
        d => d
            .TriggeredBy(Lint)
            .Before(PostLint)
            .OnlyWhenDynamic(() => LintPaths.IsLocalLintOrMatches(DotnetFormatMatcher))
            .Net9MsBuildFix()
            .ProceedAfterFailure()
            .Executes(
                 () =>
                 {
                     var formatSettings = new DotNetFormatSettings()
                                         .SetSeverity(DotNetFormatSeverity)
                                         .SetVerbosity(Verbosity.MapVerbosity(DotNetVerbosity.normal))
                                         .EnableNoRestore()
                                         .SetBinaryLog(LogsDirectory / "dotnet-format.binlog")
                         ;

                     if (DotNetFormatIncludedDiagnostics is { Count: > 0 })
                     {
                         formatSettings = formatSettings.SetProcessAdditionalArguments(
                             ["--diagnostics", .. DotNetFormatIncludedDiagnostics]
                         );
                     }

                     if (DotNetFormatExcludedDiagnostics is { Count: > 0 })
                     {
                         formatSettings = formatSettings.SetProcessAdditionalArguments(
                             ["--exclude-diagnostics", .. DotNetFormatExcludedDiagnostics]
                         );
                     }

                     if (LintPaths.Glob(DotnetFormatMatcher) is { Count: > 0 } values)
                     {
                         foreach (var group in PathGrouper.GroupPaths(values))
                         {
                             DotNetTasks.DotNetFormat(formatSettings.AddInclude(group.Select(z => z.ToString())));
                         }
                     }
                     else if (LintPaths.AllPaths.Glob(DotnetFormatMatcher) is { Count: > 0 } allFiles)
                     {
                         foreach (var group in PathGrouper.GroupPaths(allFiles))
                         {
                             DotNetTasks.DotNetFormat(formatSettings.AddInclude(group.Select(z => z.ToString())));
                         }
                     }
                     else
                         DotNetTasks.DotNetFormat(formatSettings);
                 }
             );

    /// <summary>
    ///     A list of diagnostic ids to exclude from the dotnet format
    /// </summary>
    public HashSet<string> DotNetFormatExcludedDiagnostics => _dotNetFormatExcludedDiagnostics;

    /// <summary>
    ///     A list of diagnostic ids to include in the dotnet format
    /// </summary>
    public HashSet<string>? DotNetFormatIncludedDiagnostics => _dotNetFormatIncludedDiagnostics;

    /// <summary>
    ///     The default matcher for jetbrains cleanup code
    /// </summary>
    public Matcher DotnetFormatMatcher => dnfMatcher ??= new Matcher(StringComparison.OrdinalIgnoreCase)
       .AddInclude("**/*.cs");

    /// <summary>
    ///     The default severity to use for DotNetFormat
    /// </summary>
    public DotNetFormatSeverity DotNetFormatSeverity => DotNetFormatSeverity.info;

    /// <summary>
    ///     Use the jetbrains code cleanup tool to format the code if installed
    /// </summary>
    public Target JetBrainsCleanupCode =>
        d =>
        {
            return d
                  .TriggeredBy(Lint)
                  .Before(DotnetFormat)
                  .Before(PostLint)
                  .OnlyWhenStatic(() => DotNetTool.IsInstalled("jb"))
                  // disable for local stagged runs, as it takes a long time.
                  .OnlyWhenStatic(
                       () => ( IsLocalBuild && LintPaths.Trigger != LintTrigger.Staged )
                        || !IsLocalBuild
                        || ExecutionPlan.Contains(JetBrainsCleanupCode)
                   )
                  .ProceedAfterFailure()
                  .OnlyWhenDynamic(() => LintPaths.IsLocalLintOrMatches(JetBrainsCleanupCodeMatcher))
                  .Net9MsBuildFix()
                  .Executes(
                       () =>
                       {
                           if (LintPaths.Glob(JetBrainsCleanupCodeMatcher) is { Count: > 0 } files)
                           {
                               foreach (var group in PathGrouper.GroupPaths(files))
                               {
                                   var arguments = new Arguments()
                                                  .Add("cleanupcode")
                                                  .Add(Solution.Path)
                                                  .Add("--profile={value}", JetBrainsCleanupCodeProfile)
                                                  .Add(
                                                       "--disable-settings-layers={value}",
                                                       "GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
                                                   );
                                   runCodeCleanup(
                                       arguments
                                          .Add("--include={value}", group.Select(z => z.ToString()), ';')
                                          .RenderForExecution()
                                   );
                               }
                           }
                           else if (LintPaths.AllPaths.Glob(JetBrainsCleanupCodeMatcher) is { Count: > 0 } allFiles)
                           {
                               foreach (var group in PathGrouper.GroupPaths(allFiles))
                               {
                                   var arguments = new Arguments()
                                                  .Add("cleanupcode")
                                                  .Add(Solution.Path)
                                                  .Add("--profile={value}", JetBrainsCleanupCodeProfile)
                                                  .Add(
                                                       "--disable-settings-layers={value}",
                                                       "GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
                                                   );
                                   runCodeCleanup(
                                       arguments
                                          .Add("--include={value}", group.Select(z => z.ToString()), ';')
                                          .RenderForExecution()
                                   );
                               }
                           }
                           else
                           {
                               var arguments = new Arguments()
                                              .Add("cleanupcode")
                                              .Add(Solution.Path)
                                              .Add("--profile={value}", JetBrainsCleanupCodeProfile)
                                              .Add(
                                                   "--disable-settings-layers={value}",
                                                   "GlobalAll;GlobalPerProduct;SolutionPersonal;ProjectPersonal"
                                               );
                               runCodeCleanup(arguments.RenderForStringHandler());
                           }
                       }
                   );

            static IReadOnlyCollection<Output> runCodeCleanup(ArgumentStringHandler args) => DotNetTool.GetTool("jb")(
                args,
                NukeBuild.RootDirectory,
                logOutput: true,
                logInvocation: NukeBuild.Verbosity == Verbosity.Verbose,
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                logger: static (t, s) => Log.Write(
                            t == OutputType.Err ? LogEventLevel.Error : LogEventLevel.Information,
                            s
                        )
                );
        };

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
    ///     The default profile to use for JetBrainsCleanupCode
    /// </summary>
    public string JetBrainsCleanupCodeProfile => "Full Cleanup";

    private static readonly HashSet<string> _dotNetFormatExcludedDiagnostics = new(StringComparer.OrdinalIgnoreCase)
    {
        "CS0103",
        "CS0108",
        "CS0246",
        "CS1591",
        "CA1869",
        "CS8602",
        "CS8604",
        "IDE0052",
        "IDE0060",
        "IDE0130",
        "IDE1006",
        "RCS1060",
        "RCS1093",
        "RCS1110",
        "RCS1112",
        "RCS1163",
        "RCS1175",
        "RCS1246",
        "RCS1250",
        "RCS1251",
        "RCS1264",
        "RS0026",
    };

    private static readonly HashSet<string> _dotNetFormatIncludedDiagnostics = new(StringComparer.OrdinalIgnoreCase);
    private static Matcher? dnfMatcher;
    private static Matcher? jbMatcher;
}

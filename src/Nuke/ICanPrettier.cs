using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines targets for projects that use preitter
/// </summary>
[PublicAPI]
public interface ICanPrettier : ICanLint
{
    private static Matcher? matcher;

    private static readonly Arguments _prettierBaseArgs = new Arguments().Add("exec").Add("prettier").Add("--");

    /// <summary>
    ///     The prettier target
    /// </summary>
    public Target Prettier =>
        d => d
            .TriggeredBy(Lint)
            .Before(PostLint)
            .OnlyWhenStatic(() => ( RootDirectory / ".prettierrc" ).FileExists())
            .OnlyWhenDynamic(() => LintPaths.IsLocalLintOrMatches(PrettierMatcher))
            .Executes(
                 () =>
                 {
                     var args =
                         LintPaths.Active
                             ? makeArgsForStagedFiles(LintPaths.Glob(PrettierMatcher))
                             : new Arguments().Concatenate(_prettierBaseArgs).Add(".").Add("--write");

                     ProcessTasks
                        .StartProcess(ToolPathResolver.GetPathExecutable("npm"), NukeBuild.IsLocalBuild ? "install" : "ci --ignore-scripts", NukeBuild.RootDirectory)
                        .AssertWaitForExit()
                        .AssertZeroExitCode();

                     return ProcessTasks
                           .StartProcess(
                                ToolPathResolver.GetPathExecutable("npm"),
                                args.RenderForExecution(),
                                logOutput: true,
                                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                logger: static (t, s) => Log.Write(t == OutputType.Err ? LogEventLevel.Error : LogEventLevel.Information, s),
                                logInvocation: Verbosity == Verbosity.Verbose
                            )
                           .AssertWaitForExit()
                           .AssertZeroExitCode();

                     static Arguments makeArgsForStagedFiles(IEnumerable<RelativePath> values)
                     {
                         var a = new Arguments().Concatenate(_prettierBaseArgs).Add("--write");
                         values.ForEach(x => a.Add(x));
                         return a;
                     }
                 }
             );

    /// <summary>
    ///     The default matcher for what files prettier supports with the xml plugin
    /// </summary>
    public Matcher PrettierMatcher => matcher ??= new Matcher(StringComparison.OrdinalIgnoreCase)
                                                 .AddInclude("**/*.csproj")
                                                 .AddInclude("**/*.targets")
                                                 .AddInclude("**/*.props")
                                                 .AddInclude("**/*.xml")
                                                 .AddInclude("**/*.ts")
                                                 .AddInclude("**/*.tsx")
                                                 .AddInclude("**/*.js")
                                                 .AddInclude("**/*.jsx")
                                                 .AddInclude("**/*.vue")
                                                 .AddInclude("**/*.json")
                                                 .AddInclude("**/*.yml")
                                                 .AddInclude("**/*.yaml")
                                                 .AddInclude("**/*.css")
                                                 .AddInclude("**/*.scss");
}

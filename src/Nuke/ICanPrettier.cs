using System.Collections.Immutable;

using Microsoft.Extensions.FileSystemGlobbing;

using Nuke.Common.IO;
using Nuke.Common.Tooling;

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
                     var args = makeArgsForStagedFiles(LintPaths.Active ? LintPaths.Glob(PrettierMatcher) : LintPaths.AllPaths.Glob(PrettierMatcher));

                     if (( NukeBuild.RootDirectory / "package.json" ).FileExists() && !NukeBuild.RootDirectory.ContainsDirectory("node_modules"))
                     {
                         ProcessTasks
                            .StartProcess(
                                 ToolPathResolver.GetPathExecutable("npm"),
                                 NukeBuild.IsLocalBuild ? "install" : "ci --ignore-scripts",
                                 NukeBuild.RootDirectory
                             )
                            .AssertWaitForExit()
                            .AssertZeroExitCode();
                     }

                     foreach (var group in args)
                     {
                         ProcessTasks
                            .StartProcess(
                                 ToolPathResolver.GetPathExecutable("npm"),
                                 group.RenderForExecution(),
                                 RootDirectory,
                                 logOutput: true,
                                 logInvocation: Verbosity == Verbosity.Verbose
,
                                 // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                                 logger: static (t, s) => Log.Write(t == OutputType.Err ? LogEventLevel.Error : LogEventLevel.Information, s))
                            .AssertWaitForExit()
                            .AssertZeroExitCode();
                     }

                     static IEnumerable<Arguments> makeArgsForStagedFiles(ImmutableList<RelativePath> values)
                     {
                         var args = new Arguments().Concatenate(_prettierBaseArgs);
                         if (values.Count == 0)
                         {
                             yield return args.Add("--write").Add(".");
                             yield break;
                         }

                         foreach (var paths in PathGrouper.GroupPaths(values))
                         {
                             yield return args.Add("--write").Add("{value}", paths, ' ');
                             args = new Arguments().Concatenate(_prettierBaseArgs);
                         }
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

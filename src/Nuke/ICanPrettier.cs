using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines targets for projects that use preitter
/// </summary>
[PublicAPI]
public interface ICanPrettier : ICanLint
{
    private static Matcher? matcher;

    /// <summary>
    ///     The prettier target
    /// </summary>
    public Target Prettier =>
        d => d
            .TriggeredBy(Lint)
            .Before(PostLint)
            .OnlyWhenStatic(() => ( RootDirectory / ".prettierrc" ).FileExists())
            .Executes(
                 () =>
                 {
                     var args = LintPaths.Trigger != LintTrigger.None && LintPaths.Glob(PrettierMatcher).ToArray() is { Length: > 0, } values
                         ? makeArgsForStagedFiles(values)
                         : new Arguments().Add("prettier").Add(".").Add("--write");

                     return ProcessTasks
                           .StartProcess(
                                ToolPathResolver.GetPathExecutable("npx"),
                                args.RenderForExecution() /*, logInvocation: false*/,
                                logger: (_, s) => Log.Logger.Information(s)
                            )
                           .AssertWaitForExit()
                           .AssertZeroExitCode();

                     static Arguments makeArgsForStagedFiles(IEnumerable<RelativePath> values)
                     {
                         var a = new Arguments().Add("prettier").Add("--write");
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
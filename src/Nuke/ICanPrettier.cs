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
    /// <summary>
    ///     The prettier target
    /// </summary>
    public Target Prettier =>
        d => d
            .TriggeredBy(Lint)
            .DependsOn(ResolveLintPaths)
            .Before(PostLint)
            .OnlyWhenStatic(() => ( RootDirectory / ".prettierrc" ).FileExists())
            .Executes(
                 () =>
                 {
                     var args = new Arguments()
                               .Add("prettier")
                               .Add(".")
                               .Add("--write");

                     if (LintPaths.HasPaths)
                     {
                         LintPaths.Paths.ForEach(x => args.Add(x));
                     }

                     return ProcessTasks
                           .StartProcess(
                                ToolPathResolver.GetPathExecutable("npx"),
                                args.RenderForExecution(),
                                logInvocation: false,
                                logger: (_, s) => Log.Logger.Information(s)
                            )
                           .AssertWaitForExit()
                           .AssertZeroExitCode();
                 }
             );
}
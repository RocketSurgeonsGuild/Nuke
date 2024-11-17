using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
[PublicAPI]
public interface ICanLint : IHaveGitRepository, IHaveLintTarget
{
    static IEnumerable<string> FilterFiles(TreeChanges patch)
    {
        foreach (var item in patch)
        {
            var result = item switch
                         {
                             { Status: ChangeKind.Added or ChangeKind.Modified or ChangeKind.Renamed or ChangeKind.Copied, } => item.Path, _ => null,
                         };
            if (string.IsNullOrWhiteSpace(result))
            {
                continue;
            }

            yield return item.Path;
        }
    }

    /// <summary>
    ///     The default matcher to exclude files from linting
    /// </summary>
    public static Matcher DefaultLintMatcher { get; } = ResolveLintMatcher();

    private static void WriteFileTreeWithEmoji(IEnumerable<AbsolutePath> stagedFiles)
    {
        var currentFolder = new Stack<string>();
        string? lastFolder = null;

        foreach (var parts in stagedFiles
                             .GetRelativePaths()
                             .Select(z => z.ToString().Split(['/', '\\',], StringSplitOptions.RemoveEmptyEntries))
                             .OrderByDescending(z => z.Length > 1)
                             .ThenBy(z => string.Join("/", z)))
        {
            var commonPrefixLength = 0;

            // Find the common prefix length
            while (commonPrefixLength < currentFolder.Count
                && commonPrefixLength < parts.Length - 1
                && currentFolder.ElementAt(commonPrefixLength) == parts[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Remove the non-common parts from the stack
            while (currentFolder.Count > commonPrefixLength)
            {
                _ = currentFolder.Pop();
            }

            // Add the new parts to the stack
            for (var i = commonPrefixLength; i < parts.Length - 1; i++)
            {
                currentFolder.Push(parts[i]);
            }

            var currentPath = string.Join("/", currentFolder.Reverse());
            if (currentPath != lastFolder)
            {
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                lastFolder = currentPath;
                if (!string.IsNullOrWhiteSpace(currentPath))
                {
                    Log.Information($"📂 {currentPath}");
                }
            }

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Log.Information($"{' '.Repeat(currentFolder.Count > 0 ? 4 : 0)}📄 " + parts[^1]);
        }
    }

    private static LintPaths? lintPaths;

    private static Matcher ResolveLintMatcher() =>
        new Matcher(StringComparison.OrdinalIgnoreCase)
           .AddInclude("**/*")
           .AddExclude("**/node_modules/**/*")
           .AddExclude(".idea/**/*")
           .AddExclude(".vscode/**/*")
           .AddExclude(".nuke/**/*")
           .AddExclude("**/bin/**/*")
           .AddExclude("**/obj/**/*")
           .AddExclude("**/*.g.*")
           .AddExclude("**/*.verified.*")
           .AddExclude("**/*.received.*");

    /// <summary>
    ///     The lint target
    /// </summary>
    public Target LintFiles => t => t
                                   .OnlyWhenDynamic(() => LintPaths.Active)
                                   .TryDependsOn<IHaveRestoreTarget>(a => a.Restore)
                                   .TryDependentFor<IHaveLintTarget>(a => a.Lint)
                                   .Executes(
                                        () =>
                                        {
                                            Log.Information("Linting files with trigger {Trigger}", LintPaths.Trigger);
                                            WriteFileTreeWithEmoji(LintPaths.Paths);
                                        }
                                    );

    /// <summary>
    ///     A lint target that runs last
    /// </summary>
    [ExcludeTarget]
    public Target PostLint => t => t.Unlisted().After(Lint).TriggeredBy(Lint);

    /// <summary>
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    [ExcludeTarget]
    public Target HuskyLint =>
        t => t
            .Unlisted()
            .OnlyWhenStatic(() => IsLocalBuild)
            .TriggeredBy(Lint)
            .Before(PostLint)
            .Executes(
                 () =>
                 {
                     var toolInstalled = DotNetTool.IsInstalled("husky");
                     if (!toolInstalled)
                     {
                         return;
                     }

                     var tool = DotNetTool.GetTool("husky");
                     _ = tool(
                         "run --group lint",
                         logOutput: true,
                         logInvocation: Verbosity == Verbosity.Verbose,
                         // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                         logger: static (t, s) => Log.Write(t == OutputType.Err ? LogEventLevel.Error : LogEventLevel.Information, s)
                     );
                 }
             );

    /// <summary>
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    public Target LintGitAdd =>
        t => t
            .Unlisted()
            .After(Lint)
            .TriggeredBy(PostLint)
            .Executes(
                 () =>
                 {
                     List<string> patterns = [".nuke/build.schema.json", ".github/workflows", "Readme.md",];
                     if (this is IHavePublicApis)
                     {
                         patterns.Add("**/PublicAPI.Shipped.txt");
                         patterns.Add("**/PublicAPI.Unshipped.txt");
                     }

                     if (this is ICanUpdateSolution sln)
                     {
                         patterns.Add(sln.Solution.Path);
                     }

                     if (LintPaths.Active)
                     {
                         patterns.AddRange(LintPaths.RelativePaths.Select(z => z.ToString()));
                     }

                     patterns.ForEach(static pattern => GitTasks.Git(new Arguments().Add("add").Add(pattern).RenderForExecution(), exitHandler: _ => { }));
                 }
             );

    /// <summary>
    ///     The lint paths rooted as an absolute path.
    /// </summary>
    public LintPaths LintPaths => lintPaths ??= ResolveLintPathsImpl();

    /// <summary>
    ///     The default matcher to exclude files from linting
    /// </summary>
    public Matcher LintMatcher => DefaultLintMatcher;

    /// <summary>
    ///     The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ", Name = "lint-files")]
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? Array.Empty<string>();

    private LintPaths ResolveLintPathsImpl()
    {
        using var repo = new Repository(RootDirectory);
        List<string> files = [];
        var trigger = LintTrigger.None;
        var message = "Linting all files";
        if (PrivateLintFiles.Any())
        {
            trigger = LintTrigger.Specific;
            message = "Linting only the files provided";
            files.AddRange(PrivateLintFiles);
        }
        else if (GitHubActions.Instance.IsPullRequest())
        {
            trigger = LintTrigger.PullRequest;
            message = "Linting only the files in the Pull Request";
            var diff = repo.Diff.Compare<TreeChanges>(
                repo.Branches[$"origin/{GitHubActions.Instance.BaseRef}"].Tip.Tree,
                repo.Branches[$"origin/{GitHubActions.Instance.HeadRef}"].Tip.Tree
            );
            files.AddRange(FilterFiles(diff));
        }
        else if (IsLocalBuild && FilterFiles(repo.Diff.Compare<TreeChanges>(repo.Head.Tip?.Tree, DiffTargets.Index)).ToArray() is { Length: > 0, } stagedFiles)
        {
            trigger = LintTrigger.Staged;
            message = "Linting only the staged files";
            files.AddRange(stagedFiles);
        }

        return files is { Count: > 0, }
            ? new(LintMatcher, trigger, message, files)
            : new(LintMatcher, trigger, message, [] /*GitTasks.Git("ls-files", logOutput: false, logInvocation: false).Select(z => z.Text)*/);
    }
}

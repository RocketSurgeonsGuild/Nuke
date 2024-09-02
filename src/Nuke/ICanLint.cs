using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Git;
using Rocket.Surgery.Nuke.GithubActions;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
[PublicAPI]
public interface ICanLint : IHaveGitRepository, IHaveLintTarget
{
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
                if (!string.IsNullOrWhiteSpace(currentPath)) Log.Information($"📂 {currentPath}");
            }

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Log.Information($"{' '.Repeat(currentFolder.Count > 0 ? 4 : 0)}📄 " + parts[^1]);
        }
    }

    private static LintPaths? lintPaths;

    /// <summary>
    ///     The lint target
    /// </summary>
    public Target Lint => t => t
                              .OnlyWhenDynamic(() => LintPaths.HasPaths)
                              .TryDependsOn<IHaveRestoreTarget>(a => a.Restore)
                              .Executes(
                                   () =>
                                   {
                                       Log.Information("Linting {Count} files", LintPaths.Paths.Count());
                                       WriteFileTreeWithEmoji(LintPaths.Paths);
                                   }
                               );

    /// <summary>
    ///     A lint target that runs last
    /// </summary>
    [NonEntryTarget]
    public Target PostLint => t => t.Unlisted().After(Lint).TriggeredBy(Lint);

    /// <summary>
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    [NonEntryTarget]
    public Target HuskyLint =>
        t => t
            .Unlisted()
            .TriggeredBy(Lint)
            .Before(PostLint)
            .Executes(
                 () =>
                 {
                     var toolInstalled = DotNetTool.IsInstalled("husky");
                     if (toolInstalled)
                     {
                         var tool = DotNetTool.GetTool("husky");
                         _ = tool("run --group lint" /*, logInvocation: false*/);
                     }
                 }
             );

    /// <summary>
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    [NonEntryTarget]
    public Target LintGitAdd =>
        t => t
            .Unlisted()
            .After(Lint)
            .TriggeredBy(PostLint)
            .Executes(
                 () =>
                 {
                     List<string> patterns = [".nuke/build.schema.json", ".github/workflows",];
                     if (this is IHavePublicApis)
                     {
                         patterns.Add("**/PublicAPI.Shipped.txt");
                         patterns.Add("**/PublicAPI.Unshipped.txt");
                     }

                     if (LintPaths.HasPaths)
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
    public Matcher LintMatcher => new Matcher(StringComparison.OrdinalIgnoreCase)
                                 .AddInclude("**/*")
                                 .AddExclude("**/node_modules/**/*")
                                 .AddExclude("**/bin/**/*")
                                 .AddExclude("**/obj/**/*")
                                 .AddExclude("**/*.verified.*")
                                 .AddExclude("**/*.received.*");

    /// <summary>
    ///     The files to lint, if not given lints all files
    /// </summary>
    [Parameter("The files to lint, if not given lints all files", Separator = " ", Name = "lint-files")]
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? [];

    private LintPaths ResolveLintPathsImpl()
    {
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
            files.AddRange(
                FilterFiles(
                    GitTasks
                       .Git(
                            $"diff --name-status origin/{GitHubActions.Instance.BaseRef} origin/{GitHubActions.Instance.HeadRef}",
                            logOutput: false,
                            logInvocation: false
                        )
                )
            );
        }
        else if (IsLocalBuild
              && FilterFiles(GitTasks.Git($"diff --name-status --cached", logOutput: false, logInvocation: false)).ToArray()
                     is { Length: > 0, } stagedFiles)
        {
            trigger = LintTrigger.Staged;
            message = "Linting only the staged files";
            files.AddRange(stagedFiles);
        }

        return files is { Count: > 0, }
            ? new(LintMatcher, trigger, message, files)
            : new(
                LintMatcher,
                trigger,
                message,
                GitTasks.Git($"ls-files", logOutput: false, logInvocation: false).Select(z => z.Text)
            );
    }

    static IEnumerable<string> FilterFiles(IEnumerable<Output> outputs)
    {
        foreach (var output in outputs)
        {
            var file = output.Text;
            if (file is ['D' or 'd', ..])
            {
                continue;
            }

            var filePath = file[8..].Trim();

            if (file is ['R' or 'r', _])
            {
                var renameIndex = filePath.LastIndexOf("   ", StringComparison.OrdinalIgnoreCase);
                switch (renameIndex)
                {
                    case > 0:
                        yield return filePath[renameIndex..].Trim();
                        break;
                }
                continue;
            }

            yield return filePath;
        }
    }
}

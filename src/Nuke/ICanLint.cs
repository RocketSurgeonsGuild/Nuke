using Microsoft.Extensions.FileSystemGlobbing;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tools.Git;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Adds support for linting the files in a solution or via
/// </summary>
[PublicAPI]
public interface ICanLint : IHaveGitRepository
{
    private static LintPaths _lintPaths = null!;

    private static void WriteFileTreeWithEmoji(IEnumerable<AbsolutePath> stagedFiles)
    {
        var currentFolder = new Stack<string>();
        string? lastFolder = null;

        foreach (var parts in stagedFiles
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
                Log.Information($"📂 {currentPath}");
                lastFolder = currentPath;
            }

            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            Log.Information($"{' '.Repeat(currentFolder.Count > 0 ? 4 : 0)}📄 " + parts[^1]);
        }
    }

    /// <summary>
    ///     The lint target
    /// </summary>
    public Target Lint => t => t
                              .DependsOn(ResolveLintPaths)
                              .OnlyWhenDynamic(() => LintPaths.HasPaths)
                              .Executes(
                                   () =>
                                   {
                                       Log.Information("Linting {Count} files", LintPaths.Paths.Count());
                                       WriteFileTreeWithEmoji(LintPaths.Paths);
                                   }
                               );

    /// <summary>
    ///     Resolve the lint paths based on the current environment
    /// </summary>
    public Target ResolveLintPaths => t => t
                                         .Executes(
                                              () =>
                                              {
                                                  List<string> files = [];
                                                  var message = "Linting all files";
                                                  if (PrivateLintFiles.Any())
                                                  {
                                                      message = "Linting only the files provided";
                                                      files.AddRange(PrivateLintFiles);
                                                  }
                                                  else if (GitHubActions.Instance.IsPullRequest())
                                                  {
                                                      message = "Linting only the files in the Pull Request";
                                                      files.AddRange(
                                                          GitTasks
                                                             .Git(
                                                                  $"diff --name-status origin/{GitHubActions.Instance.BaseRef} origin/{GitHubActions.Instance.HeadRef}",
                                                                  logOutput: false,
                                                                  logInvocation: false
                                                              )
                                                             .Where(z => z.Text[0] is not ('D' or 'd'))
                                                             .Select(z => z.Text[1..].Trim())
                                                      );
                                                  }
                                                  else if (IsLocalBuild
                                                        && GitTasks
                                                              .Git($"diff --name-status --cached", logOutput: false, logInvocation: false)
                                                              .Where(z => z.Text[0] is not ('D' or 'd'))
                                                              .Select(z => z.Text[1..].Trim())
                                                              .ToArray()
                                                               is { Length: > 0, } stagedFiles)
                                                  {
                                                      message = "Linting only the staged files";
                                                      files.AddRange(stagedFiles);
                                                  }
                                                  else
                                                  {
                                                      message = "Linting all files";
                                                  }

                                                  _lintPaths = files is { Count: > 0, }
                                                      ? new(LintMatcher, true, message, files)
                                                      : new(
                                                          LintMatcher,
                                                          false,
                                                          message,
                                                          GitTasks.Git($"ls-files", logOutput: false, logInvocation: false).Select(z => z.Text)
                                                      );
                                              }
                                          );

    /// <summary>
    ///     A lint target that runs last
    /// </summary>
    public Target PostLint => t => t.Unlisted().After(Lint).TriggeredBy(Lint).DependsOn(ResolveLintPaths);

    /// <summary>
    ///     A ensure only the linted files are added to the commit
    /// </summary>
    public Target HuskyLint =>
        t => t
            .Unlisted()
            .TriggeredBy(Lint)
            .DependsOn(ResolveLintPaths)
            .Before(PostLint)
            .Executes(
                 () =>
                 {
                     var toolInstalled = DotnetTool.IsInstalled("husky");
                     if (toolInstalled)
                     {
                         var tool = DotnetTool.GetTool("husky");
                         _ = tool("run --group lint");
                     }
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
                     _ = GitTasks.Git("add .nuke/build.schema.json", exitHandler: _ => { });
                     _ = GitTasks.Git("add .github/workflows/*.yml", exitHandler: _ => { });
                     if (this is IHavePublicApis)
                     {
                         _ = GitTasks.Git("add *PublicAPI.Shipped.txt *PublicAPI.Unshipped.txt", exitHandler: _ => { });
                     }

                     if (LintPaths.HasPaths)
                     {
                         _ = GitTasks.Git(
                             $"add {string.Join(" ",
                                 LintPaths
                                    .Paths
                                    .Select(z => RootDirectory.GetRelativePathTo(z))
                                    .Select(z => $"\"{z}\"")
                             )}",
                             exitHandler: _ => { }
                         );
                     }
                 }
             );

    /// <summary>
    ///     The lint paths rooted as an absolute path.
    /// </summary>
    public LintPaths LintPaths => _lintPaths ?? throw new InvalidOperationException("LintPaths has not been initialized");

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
    #pragma warning disable CA1819
    private string[] PrivateLintFiles => TryGetValue(() => PrivateLintFiles) ?? [];
    #pragma warning restore CA1819
}
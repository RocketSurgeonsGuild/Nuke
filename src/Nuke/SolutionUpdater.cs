using GlobExpressions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

namespace Rocket.Surgery.Nuke;

internal static class SolutionUpdater
{
    public static void UpdateSolution(
        Solution solution,
        IEnumerable<string> additionalRelativeFolderFilePatterns,
        IEnumerable<string> additionalConfigFolderFilePatterns,
        IEnumerable<string> additionalIgnoreFolderFilePatterns
    )
    {
        if (EnvironmentInfo.HasVariable("RSG_NUKE_LINT_STAGED")) return;
        if (solution.GetSolutionFolder("config") is not { } configFolder) configFolder = solution.AddSolutionFolder("config");

        var actions = ReplaceDotBuildFolder(solution, configFolder)
                     .Concat(ReplaceDotSolutionFolder(solution, configFolder))
                     .Concat(
                          AddConfigurationFiles(
                              solution,
                              additionalRelativeFolderFilePatterns,
                              additionalConfigFolderFilePatterns,
                              additionalIgnoreFolderFilePatterns,
                              configFolder
                          )
                      )
                     .Concat(AddNukeBuilds(solution, configFolder))
                     .Concat(NormalizePaths(solution))
            ;


        var found = false;
        foreach (var action in actions)
        {
            found = true;
            action();
        }

        if (!found) return;

        Log.Logger.Information("Updating solution to match newly found files");
        solution.Save();
    }

    private static readonly string[] _relativeFolderFilePatterns = { "**/Directory.*.props", "**/Directory.*.targets", "**/.editorconfig", };

    private static readonly string[] _configFolderFilePatterns =
    {
        "build.*", "*.yaml", "*.yml", "LICENSE", "*.md", ".git*",
        ".prettier*", "*lintstaged*", "NuGet.config", ".github/**/*", ".husky/*", ".vscode/**/*", "*.props", "*.targets",
        "package.json",
    };

    private static readonly string[] _ignoreFolderFilePatterns = { "**/node_modules/**", };

    private static IEnumerable<Action> AddNukeBuilds(Solution solution, SolutionFolder configFolder)
    {
        if (solution.Directory != NukeBuild.RootDirectory) yield break;
        var projectPaths = NukeBuild
                          .RootDirectory.GlobFiles(".build/*.csproj", "build/*.csproj", "_build/*.csproj")
                          .Where(z => solution.AllProjects.All(p => p.Path != z));

        foreach (var project in projectPaths)
        {
            yield return () => solution.AddProject(
                             project.NameWithoutExtension,
                             ProjectType.CSharpProject.FirstGuid,
                             project,
                             configurationPlatforms: new Dictionary<string, string>(),
                             solutionFolder: configFolder
                         );
        }

        var projects = solution
                      .AllProjects
                      .Where(
                           z => z.Name.Equals(".build", StringComparison.OrdinalIgnoreCase)
                            || z.Name.Equals("build", StringComparison.OrdinalIgnoreCase)
                            || z.Name.Equals("_build", StringComparison.OrdinalIgnoreCase)
                       )
                      .Where(z => z.Configurations.Count > 0);
        foreach (var project in projects)
        {
            yield return () =>
                         {
                             foreach (var item in project
                                                 .Configurations
                                                 .Where(z => z.Key.Contains(".Build.", StringComparison.OrdinalIgnoreCase))
                                                 .ToArray())
                             {
                                 project.Configurations.Remove(item.Key);
                             }
                         };
        }
    }

    private static List<Action> AddConfigurationFiles(
        Solution solution,
        IEnumerable<string> additionalRelativeFolderFilePatterns,
        IEnumerable<string> additionalConfigFolderFilePatterns,
        IEnumerable<string> additionalIgnoreFolderFilePatterns,
        SolutionFolder configFolder
    )
    {
        var ignoreGlobs = _ignoreFolderFilePatterns.Concat(additionalIgnoreFolderFilePatterns).Select(z => new Glob(z)).ToList();
        var actions = new List<Action>();
        if (solution.Directory != NukeBuild.RootDirectory) return actions;
        actions.AddRange(
            solution
               .Directory
               .GlobFiles(".config/*")
               .SelectMany(path => AddSolutionItemToFolder(configFolder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path)))
        );
        actions.AddRange(
            solution
               .Directory
               .GlobFiles(
                    _relativeFolderFilePatterns.Concat(additionalRelativeFolderFilePatterns).ToArray()
                )
               .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
               .SelectMany(path => AddSolutionItemToRelativeFolder(solution, configFolder, path))
        );
        actions.AddRange(
            solution
               .Directory
               .GlobFiles(
                    _configFolderFilePatterns.Concat(additionalConfigFolderFilePatterns).ToArray()
                )
               .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
               .SelectMany(path => AddSolutionItemToRelativeConfigFolder(solution, configFolder, path))
        );

        return actions;
    }

    private static IEnumerable<Action> NormalizePaths(Solution solution)
    {
        foreach (var folder in solution.AllSolutionFolders)
        {
            if (folder.Items.Values.All(z => ( (RelativePath)z ).ToUnixRelativePath() == z)) continue;
            if (folder.Items.Keys.All(z => ( (RelativePath)z ).ToUnixRelativePath() == z)) continue;
            yield return () =>
                         {
                             foreach (var item in folder
                                                 .Items.Where(
                                                      z => ( (RelativePath)z.Key ).ToUnixRelativePath() != z.Key
                                                       || ( (RelativePath)z.Value ).ToUnixRelativePath() != z.Value
                                                  )
                                                 .ToArray())
                             {
                                 folder.Items.Remove(item.Key);
                                 folder.Items.Add(( (RelativePath)item.Key ).ToUnixRelativePath(), ( (RelativePath)item.Value ).ToUnixRelativePath());
                             }
                         };
        }
    }

    private static IEnumerable<Action> AddSolutionItemToRelativeFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = path.Parent == NukeBuild.RootDirectory
            ? configFolder
            // ReSharper disable once NullableWarningSuppressionIsUsed
            : GetNestedFolder(solution, null, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent).ToUnixRelativePath())!;
        return AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path));
    }

    private static IEnumerable<Action> AddSolutionItemToRelativeConfigFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = GetNestedFolder(solution, configFolder, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent).ToUnixRelativePath()) ?? configFolder;
        return AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path));
    }

    private static SolutionFolder? GetNestedFolder(Solution solution, SolutionFolder? folder, UnixRelativePath path)
    {
        return path
              .ToString()
              .Split('/')
              .Where(z => !string.IsNullOrWhiteSpace(z))
              .Aggregate(
                   folder,
                   (acc, pathPart) => solution.GetSolutionFolder(pathPart)
                    ?? acc?.GetSolutionFolder(pathPart) ?? solution.AddSolutionFolder(pathPart, solutionFolder: acc)
               );
    }

    private static IEnumerable<Action> AddSolutionItemToFolder(SolutionFolder folder, UnixRelativePath path)
    {
        if (folder.Items.Values.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) yield break;
        if (folder.Items.Keys.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) yield break;
        if (folder.Items.ContainsKey(path)) yield break;
        yield return () =>
                     {
                         if (folder.Items.Values.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) return;
                         if (folder.Items.Keys.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) return;
                         if (folder.Items.ContainsKey(path)) return;
                         folder.Items.Add(path, path);
                     };
    }

    private static IEnumerable<Action> ReplaceDotBuildFolder(Solution solution, SolutionFolder configFolder)
    {
        // sunset .build folder
        if (solution.GetSolutionFolder(".build") is not { } buildFolder) yield break;
        yield return () => ReplaceFolder(solution, configFolder, buildFolder);
    }

    private static IEnumerable<Action> ReplaceDotSolutionFolder(Solution solution, SolutionFolder configFolder)
    {
        // sunset .build folder
        if (solution.GetSolutionFolder(".solution") is not { } buildFolder) yield break;
        yield return () => solution.RemoveSolutionFolder(buildFolder);
    }

    private static void ReplaceFolder(Solution solution, SolutionFolder configFolder, SolutionFolder buildFolder)
    {
        foreach (var project in buildFolder.Projects)
        {
            project.SolutionFolder = configFolder;
        }

        solution.RemoveSolutionFolder(buildFolder);

        foreach (var item in buildFolder.Items)
        {
            if (!configFolder.Items.ContainsKey(item.Key)) configFolder.Items.Add(item.Key, item.Value);
        }
    }
}
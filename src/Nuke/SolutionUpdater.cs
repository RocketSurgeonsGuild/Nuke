using GlobExpressions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
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

        ReplaceDotBuildFolder(solution, configFolder);
        ReplaceDotBuildFolder(solution, configFolder);
        ReplaceDotSolutionFolder(solution, configFolder);
        AddConfigurationFiles(
            solution,
            additionalRelativeFolderFilePatterns,
            additionalConfigFolderFilePatterns,
            additionalIgnoreFolderFilePatterns,
            configFolder
        );
        AddNukeBuilds(solution, configFolder);
        NormalizePaths(solution);

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

    private static void AddNukeBuilds(Solution solution, SolutionFolder configFolder)
    {
        if (solution.Directory != NukeBuild.RootDirectory) return;
        var projectPaths = NukeBuild
                          .RootDirectory.GlobFiles(".build/*.csproj", "build/*.csproj", "_build/*.csproj")
                          .Where(z => solution.AllProjects.All(p => p.Path != z));

        foreach (var project in projectPaths)
        {
            if (solution.GetProject(project.Name) is { }) continue;
            solution.AddProject(
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
                      .Where(z => z.Configurations.Count > 0)
                      .SelectMany(
                           project => project
                                     .Configurations
                                     .Where(z => z.Key.Contains(".Build.", StringComparison.OrdinalIgnoreCase)),
                           (project, pair) => ( Project: project, Key: pair.Key )
                       )
                      .ToArray();
        foreach (var (project, key) in projects)
        {
            project.Configurations.Remove(key);
        }
    }

    private static void AddConfigurationFiles(
        Solution solution,
        IEnumerable<string> additionalRelativeFolderFilePatterns,
        IEnumerable<string> additionalConfigFolderFilePatterns,
        IEnumerable<string> additionalIgnoreFolderFilePatterns,
        SolutionFolder configFolder
    )
    {
        var ignoreGlobs = _ignoreFolderFilePatterns.Concat(additionalIgnoreFolderFilePatterns).Select(z => new Glob(z)).ToList();

        solution
           .Directory
           .GlobFiles(".config/*")
           .ForEach(path => AddSolutionItemToFolder(configFolder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path)));

        solution
           .Directory
           .GlobFiles(_relativeFolderFilePatterns.Concat(additionalRelativeFolderFilePatterns).ToArray())
           .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
           .ForEach(path => AddSolutionItemToRelativeFolder(solution, configFolder, path));

        solution
           .Directory
           .GlobFiles(_configFolderFilePatterns.Concat(additionalConfigFolderFilePatterns).ToArray())
           .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
           .ForEach(path => AddSolutionItemToRelativeConfigFolder(solution, configFolder, path));
    }

    private static void NormalizePaths(Solution solution)
    {
        foreach (var folder in solution.AllSolutionFolders)
        {
                             if (folder.Items.Values.All(z => ( (RelativePath)z ).ToUnixRelativePath() == z)) return;
                             if (folder.Items.Keys.All(z => ( (RelativePath)z ).ToUnixRelativePath() == z)) return;
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
        }
    }

    private static void AddSolutionItemToRelativeFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = path.Parent == NukeBuild.RootDirectory
            ? configFolder
            // ReSharper disable once NullableWarningSuppressionIsUsed
            : GetNestedFolder(solution, null, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent).ToUnixRelativePath())!;
        AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path));
    }

    private static void AddSolutionItemToRelativeConfigFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = GetNestedFolder(solution, configFolder, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent).ToUnixRelativePath()) ?? configFolder;
        AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetUnixRelativePathTo(path));
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

    private static void AddSolutionItemToFolder(SolutionFolder folder, UnixRelativePath path)
    {
        if (folder.Items.Values.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) return;
        if (folder.Items.Keys.Select(z => ( (RelativePath)z ).ToUnixRelativePath().ToString()).Any(z => z == path)) return;
        if (folder.Items.ContainsKey(path)) return;
        folder.Items.Add(path, path);
    }

    private static void ReplaceDotBuildFolder(Solution solution, SolutionFolder configFolder)
    {
        // sunset .build folder
        if (solution.GetSolutionFolder(".build") is not { } buildFolder) return;
        ReplaceFolder(solution, configFolder, buildFolder);
    }

    private static void ReplaceDotSolutionFolder(Solution solution, SolutionFolder configFolder)
    {
        // sunset .build folder
        if (solution.GetSolutionFolder(".solution") is not { } buildFolder) return;
        solution.RemoveSolutionFolder(buildFolder);
    }

    private static void ReplaceFolder(Solution solution, SolutionFolder configFolder, SolutionFolder buildFolder)
    {
        foreach (var project in buildFolder.Projects)
        {
            project.SolutionFolder = configFolder;
        }

        if (solution.GetSolutionFolder(buildFolder.Name) is { } folder)
            solution.RemoveSolutionFolder(folder);

        foreach (var item in buildFolder.Items)
        {
            if (!configFolder.Items.ContainsKey(item.Key)) configFolder.Items.Add(item.Key, item.Value);
        }
    }
}

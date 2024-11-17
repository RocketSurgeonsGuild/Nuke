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
        string configFolderName,
        IEnumerable<string> additionalRelativeFolderFilePatterns,
        IEnumerable<string> additionalConfigFolderFilePatterns,
        IEnumerable<string> additionalIgnoreFolderFilePatterns
    )
    {
        if (solution.GetSolutionFolder(configFolderName) is not { } configFolder)
        {
            configFolder = solution.AddSolutionFolder(configFolderName);
        }

        _configFolder = configFolder;

        NormalizePaths(solution);
        AddConfigurationFiles(
            solution,
            additionalRelativeFolderFilePatterns,
            additionalConfigFolderFilePatterns,
            additionalIgnoreFolderFilePatterns,
            configFolder
        );
        AddNukeBuilds(solution, configFolder);
        NormalizePaths(solution);
        while (CleanupSolution(solution)) { }

        Log.Logger.Information("Saving solution to contain newly found files");
        solution.Save();
    }

    private static SolutionFolder _configFolder;

    private static readonly string[] _relativeFolderFilePatterns = ["**/Directory.*.props", "**/Directory.*.targets", "**/.editorconfig",];

    private static readonly string[] _configFolderFilePatterns =
    [
        "build.*", "*.yaml", "*.yml", "LICENSE", "*.md", ".git*",
        ".prettier*", "*lintstaged*", "NuGet.config", ".github/**/*", ".husky/*", ".vscode/**/*", "*.props", "*.targets",
        "package.json",
    ];

    private static readonly string[] _ignoreFolderFilePatterns = ["**/node_modules/**", "**/.idea/**",,];

    private static void AddNukeBuilds(Solution solution, SolutionFolder configFolder)
    {
        if (solution.Directory != NukeBuild.RootDirectory)
        {
            return;
        }

        var projectPaths = NukeBuild
                          .RootDirectory.GlobFiles("*build/*.csproj")
                          .Where(z => solution.AllProjects.All(p => p.Path != z))
                          .ToArray();

        foreach (var project in projectPaths)
        {
            if (solution.GetProject(project.Name) is { })
            {
                continue;
            }

            _ = solution.AddProject(
                project.NameWithoutExtension,
                ProjectType.CSharpProject.FirstGuid,
                project,
                configurationPlatforms: new Dictionary<string, string>(),
                solutionFolder: configFolder
            );
        }

        var projects = solution
                      .AllProjects.Join(projectPaths, z => z.Path, z => z, (project, path) => project)
                      .Where(z => z.Configurations.Count > 0)
                      .SelectMany(
                           project => project.Configurations.Where(z => z.Key.Contains(".Build.", StringComparison.OrdinalIgnoreCase)),
                           (project, pair) => ( Project: project, pair.Key )
                       )
                      .ToArray();
        foreach (( var project, var key ) in projects)
        {
            Log.Logger.Information("Removing {Key} from {Project} configuration", key, project.Name);
            _ = project.Configurations.Remove(key);
        }
    }

    private static bool CleanupSolution(Solution solution)
    {
        var implicitConfigItems = solution.Directory.GlobFiles(".config/*").ToHashSet();

        var done = false;
        var itemValuesToRemove = solution
                                .AllSolutionFolders
                                .SelectMany(z => z.Items, (folder, pair) => ( Folder: folder, ItemPath: pair.Key, FilePath: solution.Directory / pair.Value ))
                                .Where(z => !z.FilePath.FileExists() && !implicitConfigItems.Contains(z.FilePath))
                                .ToArray();
        foreach (( var folder, var itemPath, _ ) in itemValuesToRemove)
        {
            done = true;
            Log.Logger.Information("Removing {ItemPath} from {Folder}", GetItemRelativePath(folder, itemPath), GetSolutionFolderPath(folder));
            _ = folder.Items.Remove(itemPath);
        }

        var itemKeysToRemove = solution
                              .AllSolutionFolders
                              .SelectMany(
                                   z => z.Items,
                                   (folder, pair) => ( Folder: folder, ItemPath: pair.Key,
                                                       FilePath: GetItemPath(solution, GetItemRelativeFilePath(folder, pair.Key)),
                                                       RealFilePath: solution.Directory / pair.Value )
                               )
                              .Where(z => !z.FilePath.FileExists() && !implicitConfigItems.Contains(z.RealFilePath))
                              .ToArray();
        foreach (( var folder, var itemPath, _, _ ) in itemKeysToRemove)
        {
            done = true;
            Log.Logger.Information("Removing {ItemPath} from {Folder}", GetItemRelativePath(folder, itemPath), GetSolutionFolderPath(folder));
            _ = folder.Items.Remove(itemPath);
        }

        var emptyFoldersToRemove = solution
                                  .AllSolutionFolders
                                  .Where(z => z.Items.Count == 0 && z.Projects.Count == 0 && z.SolutionFolders.Count == 0)
                                  .ToArray();
        foreach (var folder in emptyFoldersToRemove)
        {
            done = true;
            Log.Logger.Information("Removing {Folder}", GetSolutionFolderPath(folder));
            _ = solution.RemoveSolutionFolder(folder);
        }

        return done;
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
           .ForEach(path => AddSolutionItemToFolder(configFolder, NukeBuild.RootDirectory.GetRelativePathTo(path)));

        solution
           .Directory
           .GlobFiles([.. _relativeFolderFilePatterns, .. additionalRelativeFolderFilePatterns,,])
           .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
           .ForEach(path => AddSolutionItemToRelativeFolder(solution, configFolder, path));

        solution
           .Directory
           .GlobFiles([.. _configFolderFilePatterns, .. additionalConfigFolderFilePatterns,,])
           .Where(path => ignoreGlobs.All(z => !z.IsMatch(path)))
           .ForEach(path => AddSolutionItemToRelativeConfigFolder(solution, configFolder, path));
    }

    private static void NormalizePaths(Solution solution)
    {
        foreach (var folder in solution.AllSolutionFolders)
        {
            foreach (var item in folder.Items.ToArray())
            {
                _ = folder.Items.Remove(item.Key);
                folder.Items.Add(toSolutionPath(item.Key), toSolutionPath(item.Value));
            }
        }

        static string toSolutionPath(string path)
        {
            return path.Replace('/', '\\');
        }

        static bool isUsingSolutionPath(KeyValuePair<string, string> item)
        {
            return item.Value.Contains('\\');
        }
    }

    private static void AddSolutionItemToRelativeFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = path.Parent == NukeBuild.RootDirectory
            ? configFolder
            // ReSharper disable once NullableWarningSuppressionIsUsed
            : GetNestedFolder(solution, null, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent))!;
        AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetRelativePathTo(path));
    }

    private static void AddSolutionItemToRelativeConfigFolder(Solution solution, SolutionFolder configFolder, AbsolutePath path)
    {
        var folder = GetNestedFolder(solution, configFolder, NukeBuild.RootDirectory.GetRelativePathTo(path.Parent)) ?? configFolder;
        AddSolutionItemToFolder(folder, NukeBuild.RootDirectory.GetRelativePathTo(path));
    }

    private static SolutionFolder? GetNestedFolder(Solution solution, SolutionFolder? placedInto, RelativePath path) => path
       .ToString()
       .Split(['/', '\\',], StringSplitOptions.RemoveEmptyEntries)
       .Aggregate(
            placedInto,
            (parent, pathPart) =>
            {
                var folder = parent is null ? solution.GetSolutionFolder(pathPart) : parent.GetSolutionFolder(pathPart);
                return folder ?? solution.AddSolutionFolder(pathPart, solutionFolder: parent);
            }
        );

    private static void AddSolutionItemToFolder(SolutionFolder folder, RelativePath path)
    {
        path = path.ToWinRelativePath();
        var key = Path.GetFileName(path);
        if (folder.Items.ContainsKey(key))
        {
            return;
        }

        Log.Logger.Information("Adding {Path} to {Folder}", path, GetSolutionFolderPath(folder));
        folder.Items.Add(key, path);
    }

    private static AbsolutePath GetItemPath(Solution solution, RelativePath relativePath) => solution.Directory / relativePath;

    private static RelativePath GetItemRelativeFilePath(SolutionFolder folder, string path) => GetSolutionFolderPath(folder, true) / path;

    private static RelativePath GetItemRelativePath(SolutionFolder folder, string path) => GetSolutionFolderPath(folder) / path;

    private static RelativePath GetSolutionFolderPath(SolutionFolder folder, bool withoutConfig = false)
    {
        var parts = new List<string>();
        while (folder is { })
        {
            if (withoutConfig && ( folder == _configFolder || ( folder.Name == _configFolder.Name && folder.SolutionFolder is null ) ))
            {
                break;
            }

            parts.Insert(0, folder.Name);
            folder = folder.SolutionFolder;
        }

        return ( (RelativePath)string.Join("/", parts) ).ToWinRelativePath();
    }
}

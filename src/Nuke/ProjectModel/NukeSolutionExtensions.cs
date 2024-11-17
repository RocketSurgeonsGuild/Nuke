using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Nuke.Common.ProjectModel;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

#pragma warning disable CA2255

namespace Rocket.Surgery.Nuke.ProjectModel;

public static class NukeSolutionExtensions
{
    public static MsbProject AnalyzeProject(this Project project)
    {
        if (_projects.TryGetValue(project, out var msbProject))
        {
            return msbProject;
        }

        _projects[project] = msbProject = MsbProject.LoadProject(project.Path);
        return msbProject;
    }

    public static IEnumerable<MsbProject> AnalyzeAllProjects(this Solution solution)
    {
        return solution.AllProjects.Select(x => x.AnalyzeProject());
    }

    [ModuleInitializer]
    public static void Initialize()
    {
        if (!MSBuildLocator.CanRegister)
            return;

        try
        {
            MSBuildLocator.RegisterDefaults();
            triggerAssemblyResolution();
        }
        catch (Exception exception)
        {
            Log.Warning("Could not register MSBuild: {Message}", exception.Message);
        }

        return;

        static void triggerAssemblyResolution()
        {
            _ = new ProjectCollection();
        }
    }

    internal static Microsoft.Build.Evaluation.Project ParseProject(string projectFile, string? configuration = null, string? targetFramework = null)
    {
        var sw = Stopwatch.StartNew();
        var projectCollection = new ProjectCollection();
        var projectRoot = ProjectRootElement.Open(projectFile, projectCollection, true);
        var msbuildProject = Microsoft.Build.Evaluation.Project.FromProjectRootElement(
            projectRoot,
            new()
            {
                GlobalProperties = GetProperties(configuration, targetFramework),
                ToolsVersion = projectCollection.DefaultToolsVersion,
                ProjectCollection = projectCollection,
            }
        );

        var targetFrameworks = msbuildProject
                              .AllEvaluatedItems
                              .Where(x => x.ItemType == "_TargetFrameworks")
                              .Select(x => x.EvaluatedInclude)
                              .OrderBy(x => x)
                              .ToList();

        if (targetFramework is null && targetFrameworks is { Count: > 0 })
        {
            projectCollection.UnloadProject(msbuildProject);
            targetFramework = targetFrameworks[0];

            Log.Warning("Project {Project} has multiple target frameworks {@TargetFrameworks}", projectFile, targetFrameworks);
            Log.Warning("Evaluating using {TargetFramework} ...", targetFramework);

            msbuildProject = new(
                projectFile,
                GetProperties(configuration, targetFramework),
                projectCollection.DefaultToolsVersion,
                projectCollection
            );
        }

        sw.Stop();
        Log.Information("Parsed project {Project} in {Elapsed}", projectFile, sw.Elapsed);

        return msbuildProject;
    }

    private static readonly ConcurrentDictionary<Project, MsbProject> _projects = new();

    private static Dictionary<string, string> GetProperties(string? configuration, string? targetFramework)
    {
        var properties = new Dictionary<string, string>();
        if (configuration != null)
            properties.Add("Configuration", configuration);
        if (targetFramework != null)
            properties.Add("TargetFramework", targetFramework);
        return properties;
    }
}

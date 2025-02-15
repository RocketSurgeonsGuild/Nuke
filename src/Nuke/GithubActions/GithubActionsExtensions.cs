using System.Collections.Concurrent;
using System.Reflection;

using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Helper functions for creating github actions with a nuke build
/// </summary>
[PublicAPI]
public static class GithubActionsExtensions
{
    /// <summary>
    ///     Adds a new step to the current configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="job"></param>
    /// <returns></returns>
    public static RocketSurgeonGitHubActionsConfiguration AddJob(
        this RocketSurgeonGitHubActionsConfiguration configuration,
        RocketSurgeonsGithubActionsJobBase job
    )
    {
        configuration.Jobs.Add(job);
        return configuration;
    }

    /// <summary>
    ///     Add nuget caching
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob AddNuGetCache(this RocketSurgeonsGithubActionsJob job)
    {
        job.Environment["NUGET_PACKAGES"] = "${{ github.workspace }}/.nuget/packages";
        job.InsertAfterCheckOut(
            new UsingStep("NuGet Cache")
            {
                Uses = "actions/cache@v3",
                With =
                {
                    ["path"] = "${{ github.workspace }}/.nuget/packages",
                    // keep in mind using central package versioning here
                    ["key"] = "${{ runner.os }}-nuget-${{ hashFiles('**/Directory.Packages.props') }}-${{ hashFiles('**/Directory.Packages.support.props') }}",
                    ["restore-keys"] = @"|
              ${{ runner.os }}-nuget-",
                },
            }
        );
        return job;
    }

    /// <summary>
    ///     Adds the default publish nuget step to the given configuration
    /// </summary>
    /// <param name="build"></param>
    /// <param name="secret"></param>
    /// <returns></returns>
    [Obsolete("Method no longer does anything, use PublishNugetJob attribute instead")]
    public static RocketSurgeonGitHubActionsConfiguration AddNugetPublish(this RocketSurgeonGitHubActionsConfiguration build, string secret) => build;

    /// <summary>
    ///     Adds a new step to the current job
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob AddStep(this RocketSurgeonsGithubActionsJob configuration, GitHubActionsStep step)
    {
        configuration.Steps.Add(step);
        return configuration;
    }

    /// <summary>
    ///     Ensure gitversion is configured to fetch all.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob ConfigureForGitVersion(this RocketSurgeonsGithubActionsJob job)
    {
        job.InsertAfterCheckOut(new RunStep("Fetch all history for all tags and branches") { Run = "git fetch --prune" });
        return job;
    }

    /// <summary>
    ///     Configures a give step using the delegate
    /// </summary>
    /// <param name="job"></param>
    /// <param name="configure"></param>
    /// <typeparam name="TStep"></typeparam>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob ConfigureStep<TStep>(this RocketSurgeonsGithubActionsJob job, Action<TStep> configure)
        where TStep : GitHubActionsStep
    {
        foreach (var step in job.Steps.OfType<TStep>())
        {
            configure(step);
        }

        return job;
    }

    /// <summary>
    ///     Adds paths that should be excluded from triggering a full CI build in github actions
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static RocketSurgeonGitHubActionsConfiguration ExcludePaths(this RocketSurgeonGitHubActionsConfiguration configuration, params string[] paths)
    {
        foreach (var item in configuration.DetailedTriggers.OfType<RocketSurgeonGitHubActionsVcsTrigger>())
        {
            item.ExcludePaths = [.. item.IncludePaths.Concat(paths).Distinct()];
        }

        return configuration;
    }

    /// <summary>
    ///     Adds common paths that should be excluded from triggering a full CI build in github actions
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static RocketSurgeonGitHubActionsConfiguration ExcludeRepositoryConfigurationFiles(this RocketSurgeonGitHubActionsConfiguration configuration) =>
        ExcludePaths(configuration, _pathsIgnore);

    /// <summary>
    ///     Adds paths that should be included to trigger a full CI build in github actions
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static RocketSurgeonGitHubActionsConfiguration IncludePaths(this RocketSurgeonGitHubActionsConfiguration configuration, params string[] paths)
    {
        foreach (var item in configuration.DetailedTriggers.OfType<RocketSurgeonGitHubActionsVcsTrigger>())
        {
            item.IncludePaths = [.. item.IncludePaths.Concat(paths).Distinct()];
        }

        return configuration;
    }

    /// <summary>
    ///     Adds common paths that should be included to trigger a full CI build in github actions
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static RocketSurgeonGitHubActionsConfiguration IncludeRepositoryConfigurationFiles(this RocketSurgeonGitHubActionsConfiguration configuration) =>
        IncludePaths(configuration, _pathsIgnore);

    /// <summary>
    ///     Adds a new step after the checkout step
    /// </summary>
    /// <param name="job"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob InsertAfterCheckOut(this RocketSurgeonsGithubActionsJob job, GitHubActionsStep step)
    {
        job.Steps.Insert(getCheckStepIndex(job) + 1, step);
        return job;

        static int getCheckStepIndex(RocketSurgeonsGithubActionsJob job)
        {
            var checkoutStep = job.Steps.OfType<CheckoutStep>().SingleOrDefault();
            return checkoutStep is null ? 1 : job.Steps.IndexOf(checkoutStep);
        }
    }

    /// <summary>
    ///     Defines a target that produces a certian github actions output
    /// </summary>
    /// <param name="target"></param>
    /// <param name="outputName"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static ITargetDefinition ProducesGithubActionsOutput(this ITargetDefinition target, string outputName, string? description = null)
    {
        if (!outputPaths.TryGetValue(target, out var paths))
        {
            paths = [];
            outputPaths[target] = paths;
        }

        paths.Add(new(outputName, description));
        return target;
    }

    /// <summary>
    ///     Publishes standard logging based on the interfaces used by the build
    /// </summary>
    /// <param name="job"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob PublishArtifacts<T>(this RocketSurgeonsGithubActionsJob job) where T : INukeBuild
    {
        // fallback for projects not yet calling publish artifacts
        if (typeof(IHaveNuGetPackages).IsAssignableFrom(typeof(T)) && !job.InternalData.TryGetValue(typeof(IHaveNuGetPackages), out _))
        {
            AddStep(
                job,
                new UploadArtifactStep("Publish NuGet Packages")
                {
                    Name = "nuget",
                    Path = "artifacts/nuget/",
                    If = "always()",
                }
            );
            job.InternalData[typeof(IHaveNuGetPackages)] = true;
        }

        if (typeof(IGenerateDocFx).IsAssignableFrom(typeof(T)) && !job.InternalData.TryGetValue(typeof(IGenerateDocFx), out _))
        {
            AddStep(
                job,
                new UploadArtifactStep("Publish Documentation")
                {
                    Name = "docs",
                    Path = "artifacts/docs/",
                }
            );
            job.InternalData[typeof(IGenerateDocFx)] = true;
        }

        return job;
    }

    /// <summary>
    ///     Publishes standard logging based on the interfaces used by the build
    /// </summary>
    /// <param name="job"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob PublishLogs<T>(this RocketSurgeonsGithubActionsJob job) where T : INukeBuild
    {
        if (typeof(IHaveOutputLogs).IsAssignableFrom(typeof(T)))
        {
            AddStep(
                job,
                new UploadArtifactStep("Publish logs")
                {
                    Name = "logs",
                    Path = "artifacts/logs/",
                    If = "always()",
                }
            );
        }

        if (typeof(IHaveTestArtifacts).IsAssignableFrom(typeof(T)))
        {
            AddStep(
                job,
                new UploadArtifactStep("Publish test data")
                {
                    Name = "test data",
                    Path = "artifacts/test/",
                    If = "always()",
                }
            );

            job.Permissions ??= new();
            job.Permissions.Checks = GitHubActionsPermission.Write;
            job.Permissions.PullRequests = GitHubActionsPermission.Write;
            job.Permissions.Contents = job.Permissions.Contents == GitHubActionsPermission.None ? GitHubActionsPermission.Read : job.Permissions.Contents;
            job.Permissions.Issues = job.Permissions.Issues == GitHubActionsPermission.None ? GitHubActionsPermission.Read : job.Permissions.Issues;

            AddStep(
                job,
                new UsingStep("Publish Test Results")
                {
                    Uses = "EnricoMi/publish-unit-test-result-action@v2",
                    If = "always()",
                    With = new()
                    {
                        ["files"] = "artifacts/test/**/*.trx",
                        // TODO: Matrix support?
                        // [""] = "Test Results"
                    },
                }
            );
        }

        if (typeof(IHaveCodeCoverage).IsAssignableFrom(typeof(T)))
        {
            AddStep(
                job,
                new UploadArtifactStep("Publish coverage data")
                {
                    Name = "coverage",
                    Path = "coverage/",
                    If = "always()",
                }
            );

            AddStep(
                job,
                new StickyPullRequestStep("Publish Coverage Comment")
                {
                    If = "always() && github.event_name == 'pull_request'",
                    ContinueOnError = true,
                    Header = "Coverage",
                    Path = "coverage/summary/SummaryGithub.md",
                }
            );

            if (DotNetTool.IsInstalled("codecov.tool"))
            {
                AddStep(
                    job,
                    new UsingStep("Publish Codecov Coverage")
                    {
                        Uses = "codecov/codecov-action@v4",
                        If =
                            "always() && (github.event_name != 'pull_request' && github.event_name != 'pull_request_target') || ((github.event_name == 'pull_request' || github.event_name == 'pull_request_target') && github.event.pull_request.user.login != 'renovate[bot]' && github.event.pull_request.user.login != 'dependabot[bot]')",
                        ContinueOnError = true,
                        With = new()
                        {
                            ["name"] = "actions",
                            // TODO: Matrix support?
                            // [""] = "Test Results"
                            ["token"] = "${{ secrets.CODECOV_TOKEN }}",
                        },
                    }
                );
            }
        }

        PublishArtifacts<T>(job);

        return job;
    }

    /// <summary>
    ///     Set an output for github actions
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static GitHubActions SetOutput(this GitHubActions instance, string key, string? value)
    {
        var outputFile = EnvironmentInfo.GetVariable<string>("GITHUB_OUTPUT");
        File.AppendAllText(outputFile, $"{key}={value}{Environment.NewLine}");
        return instance;
    }

    /// <summary>
    ///     Use a specific dotnet sdk
    /// </summary>
    /// <param name="job"></param>
    /// <param name="version"></param>
    /// <param name="exactVersion"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob UseDotNetSdk(this RocketSurgeonsGithubActionsJob job, string version, string? exactVersion = null)
    {
        exactVersion ??= version + ".x";
        job.InsertAfterCheckOut(new SetupDotNetStep($"Use .NET Core {version} SDK") { DotNetVersion = exactVersion });
        return job;
    }

    /// <summary>
    ///     Use a set of dotnet sdks
    /// </summary>
    /// <param name="job"></param>
    /// <param name="versions"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob UseDotNetSdks(this RocketSurgeonsGithubActionsJob job, params string[] versions)
    {
        foreach (var version in versions.AsEnumerable().Reverse())
        {
            job.UseDotNetSdk(version);
        }

        return job;
    }

    internal static List<GitHubActionsOutput> GetGithubActionsOutput(ExecutableTarget target)
    {
        // ReSharper disable once NullableWarningSuppressionIsUsed
        var def = (ITargetDefinition)DefinitionProperty.GetValue(target)!;
        if (outputPaths.TryGetValue(def, out var paths)) return paths;

        paths = [];
        outputPaths[def] = paths;
        return paths;
    }

    private static readonly string[] _pathsIgnore =
    [
        ".codecov.yml",
        ".editorconfig",
        ".gitattributes",
        ".gitignore",
        ".gitmodules",
        ".lintstagedrc.js",
        ".prettierignore",
        ".prettierrc",
        "LICENSE",
        "nukeeper.settings.json",
        "omnisharp.json",
        "package-lock.json",
        "package.json",
        "Readme.md",
        ".github/dependabot.yml",
        ".github/labels.yml",
        ".github/release.yml",
        ".github/renovate.json",
    ];

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private static readonly PropertyInfo DefinitionProperty =
        typeof(ExecutableTarget).GetProperty("Definition", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static readonly ConcurrentDictionary<ITargetDefinition, List<GitHubActionsOutput>> outputPaths = new();
}

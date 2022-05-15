using Nuke.Common.CI.GitHubActions.Configuration;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Helper functions for creating github actions with a nuke build
/// </summary>
[PublicAPI]
public static class GithubActionsExtensions
{
    /// <summary>
    ///     Add nuget caching
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob AddNuGetCache(this RocketSurgeonsGithubActionsJob job)
    {
        job.Environment["NUGET_PACKAGES"] = "${{ github.workspace }}/.nuget/packages";
        job.Steps.Insert(
            GetCheckStepIndex(job) + 1,
            new UsingStep("NuGet Cache")
            {
                Uses = "actions/cache@v2",
                With =
                {
                    ["path"] = "${{ github.workspace }}/.nuget/packages",
                    // keep in mind using central package versioning here
                    ["key"] = "${{ runner.os }}-nuget-${{ hashFiles('**/Directory.Packages.props') }}-${{ hashFiles('**/Directory.Packages.support.props') }}",
                    ["restore-keys"] = @"|
              ${{ runner.os }}-nuget-"
                }
            }
        );
        return job;
    }

    /// <summary>
    ///     Ensure gitversion is configured to fetch all.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob ConfigureForGitVersion(this RocketSurgeonsGithubActionsJob job)
    {
        var checkoutStep = job.Steps.OfType<CheckoutStep>().SingleOrDefault();
        if (checkoutStep is null) return job;
        job.Steps.Insert(
            job.Steps.IndexOf(checkoutStep), new RunStep("Fetch all history for all tags and branches")
            {
                Run = "git fetch --prune"
            }
        );
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
        if (typeof(IHaveCodeCoverage).IsAssignableFrom(typeof(T)))
        {
            job.Steps.Add(
                new UploadArtifactStep("Publish coverage data")
                {
                    Name = "coverage",
                    Path = "coverage/",
                    If = "always()"
                }
            );

            if (Helpers.IsDotnetToolInstalled("codecov.tool"))
            {
                job.Steps.Add(
                    new UsingStep("Publish Coverage")
                    {
                        Uses = "codecov/codecov-action@v1",
                        If =
                            "github.event_name != 'pull_request' && github.event_name != 'pull_request_target') || ((github.event_name == 'pull_request' || github.event_name == 'pull_request_target') && github.event.pull_request.user.login != 'renovate[bot]' && github.event.pull_request.user.login != 'dependabot[bot]'",
                        With = new() { ["name"] = "actions-${{ matrix.os }}" }
                    }
                );
            }
        }

        if (typeof(IHaveOutputLogs).IsAssignableFrom(typeof(T)))
        {
            job.Steps.Add(
                new UploadArtifactStep("Publish logs")
                {
                    Name = "logs",
                    Path = "artifacts/logs/",
                    If = "always()"
                }
            );
        }

        if (typeof(IHaveTestArtifacts).IsAssignableFrom(typeof(T)))
        {
            job.Steps.Add(
                new UploadArtifactStep("Publish test data")
                {
                    Name = "test data",
                    Path = "artifacts/test/",
                    If = "always()"
                }
            );
        }

        if (typeof(IHaveNuGetPackages).IsAssignableFrom(typeof(T)))
        {
            job.Steps.Add(
                new UploadArtifactStep("Publish NuGet Packages")
                {
                    Name = "nuget",
                    Path = "artifacts/nuget/",
                    If = "always()"
                }
            );
        }

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
    ///     Use a set of dotnet sdks
    /// </summary>
    /// <param name="job"></param>
    /// <param name="versions"></param>
    /// <returns></returns>
    public static RocketSurgeonsGithubActionsJob UseDotNetSdks(this RocketSurgeonsGithubActionsJob job, params string[] versions)
    {
        foreach (var version in versions.Reverse())
        {
            job.UseDotNetSdk(version);
        }

        return job;
    }

    private static int GetCheckStepIndex(RocketSurgeonsGithubActionsJob job)
    {
        var checkoutStep = job.Steps.OfType<CheckoutStep>().SingleOrDefault();
        return checkoutStep is null ? 1 : job.Steps.IndexOf(checkoutStep);
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
        job.Steps.Insert(
            GetCheckStepIndex(job) + 1,
            new SetupDotNetStep($"Use .NET Core {version} SDK")
            {
                DotNetVersion = exactVersion
            }
        );
        return job;
    }
}

using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     The permissions collections object
/// </summary>
public record GitHubActionsPermissions
{
    /// <summary>
    ///     Write the permissions to the given yaml file
    /// </summary>
    /// <param name="writer"></param>
    public void Write(CustomFileWriter writer)
    {
        if (this == None)
        {
            writer.WriteLine("permissions: {}");
            return;
        }

        if (this == WriteAll)
        {
            writer.WriteLine("permissions: write-all");
            return;
        }

        if (this == ReadAll)
        {
            writer.WriteLine("permissions: read-all");
            return;
        }

        writer.WriteLine("permissions:");
        using (writer.Indent())
        {
            if (Actions is { } actions) writer.WriteLine($"actions: {actions.GetValue()}");

            if (Checks is { } checks) writer.WriteLine($"checks: {checks.GetValue()}");

            if (Contents is { } contents) writer.WriteLine($"contents: {contents.GetValue()}");

            if (Deployments is { } deployments) writer.WriteLine($"deployments: {deployments.GetValue()}");

            if (IdToken is { } idToken) writer.WriteLine($"id-token: {idToken.GetValue()}");

            if (Issues is { } issues) writer.WriteLine($"issues: {issues.GetValue()}");

            if (Discussions is { } discussions) writer.WriteLine($"discussions: {discussions.GetValue()}");

            if (Packages is { } packages) writer.WriteLine($"packages: {packages.GetValue()}");

            if (Pages is { } pages) writer.WriteLine($"pages: {pages.GetValue()}");

            if (PullRequests is { } pullRequests) writer.WriteLine($"pull-requests: {pullRequests.GetValue()}");

            if (RepositoryProjects is { } repositoryProjects) writer.WriteLine($"repository-projects: {repositoryProjects.GetValue()}");

            if (SecurityEvents is { } securityEvents) writer.WriteLine($"security-events: {securityEvents.GetValue()}");

            if (Statuses is { } statuses) writer.WriteLine($"statuses: {statuses.GetValue()}");
        }
    }

    /// <summary>
    ///     No permissions to anything
    /// </summary>
    public static GitHubActionsPermissions None { get; } = new()
    {
        Actions = GitHubActionsPermission.None,
        Checks = GitHubActionsPermission.None,
        Contents = GitHubActionsPermission.None,
        Deployments = GitHubActionsPermission.None,
        IdToken = GitHubActionsPermission.None,
        Issues = GitHubActionsPermission.None,
        Discussions = GitHubActionsPermission.None,
        Packages = GitHubActionsPermission.None,
        Pages = GitHubActionsPermission.None,
        PullRequests = GitHubActionsPermission.None,
        RepositoryProjects = GitHubActionsPermission.None,
        SecurityEvents = GitHubActionsPermission.None,
        Statuses = GitHubActionsPermission.None,
    };

    /// <summary>
    ///     Write all permissions
    /// </summary>
    public static GitHubActionsPermissions WriteAll { get; } = new()
    {
        Actions = GitHubActionsPermission.Write,
        Checks = GitHubActionsPermission.Write,
        Contents = GitHubActionsPermission.Write,
        Deployments = GitHubActionsPermission.Write,
        IdToken = GitHubActionsPermission.Write,
        Issues = GitHubActionsPermission.Write,
        Discussions = GitHubActionsPermission.Write,
        Packages = GitHubActionsPermission.Write,
        Pages = GitHubActionsPermission.Write,
        PullRequests = GitHubActionsPermission.Write,
        RepositoryProjects = GitHubActionsPermission.Write,
        SecurityEvents = GitHubActionsPermission.Write,
        Statuses = GitHubActionsPermission.Write,
    };

    /// <summary>
    ///     Read all permissions
    /// </summary>
    public static GitHubActionsPermissions ReadAll { get; } = new()
    {
        Actions = GitHubActionsPermission.Read,
        Checks = GitHubActionsPermission.Read,
        Contents = GitHubActionsPermission.Read,
        Deployments = GitHubActionsPermission.Read,
        IdToken = GitHubActionsPermission.Read,
        Issues = GitHubActionsPermission.Read,
        Discussions = GitHubActionsPermission.Read,
        Packages = GitHubActionsPermission.Read,
        Pages = GitHubActionsPermission.Read,
        PullRequests = GitHubActionsPermission.Read,
        RepositoryProjects = GitHubActionsPermission.Read,
        SecurityEvents = GitHubActionsPermission.Read,
        Statuses = GitHubActionsPermission.Read,
    };

    /// <summary>
    ///     The actions
    /// </summary>
    public GitHubActionsPermission Actions { get; set; } = GitHubActionsPermission.Read;

    /// <summary>
    ///     The checks
    /// </summary>
    public GitHubActionsPermission Checks { get; set; } = GitHubActionsPermission.Read;

    /// <summary>
    ///     The contents
    /// </summary>
    public GitHubActionsPermission Contents { get; set; } = GitHubActionsPermission.Read;

    /// <summary>
    ///     The deployments
    /// </summary>
    public GitHubActionsPermission Deployments { get; set; } = GitHubActionsPermission.Read;

    /// <summary>
    ///     The id-token
    /// </summary>
    public GitHubActionsPermission IdToken { get; set; }

    /// <summary>
    ///     The issues
    /// </summary>
    public GitHubActionsPermission Issues { get; set; } = GitHubActionsPermission.Write;

    /// <summary>
    ///     The discussions
    /// </summary>
    public GitHubActionsPermission Discussions { get; set; }

    /// <summary>
    ///     The packages
    /// </summary>
    public GitHubActionsPermission Packages { get; set; }

    /// <summary>
    ///     The pages
    /// </summary>
    public GitHubActionsPermission Pages { get; set; }

    /// <summary>
    ///     The pull-requests
    /// </summary>
    public GitHubActionsPermission PullRequests { get; set; } = GitHubActionsPermission.Write;

    /// <summary>
    ///     The repository-projects
    /// </summary>
    public GitHubActionsPermission RepositoryProjects { get; set; }

    /// <summary>
    ///     The security-events
    /// </summary>
    public GitHubActionsPermission SecurityEvents { get; set; }

    /// <summary>
    ///     The statuses
    /// </summary>
    public GitHubActionsPermission Statuses { get; set; } = GitHubActionsPermission.Write;
}

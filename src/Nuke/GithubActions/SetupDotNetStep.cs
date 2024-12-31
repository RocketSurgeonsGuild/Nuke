#pragma warning disable CA1056
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A wrapper around the SetupDotNet Step
/// </summary>
[PublicAPI]
public class SetupDotNetStep : UsingStep
{
    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public SetupDotNetStep(string name) : base(name) =>
        // temporary to deal with sxs issue roll back once https://github.com/actions/setup-dotnet/pull/71 is merged
        Uses = "actions/setup-dotnet@v4";

    /// <summary>SDK version to use. Example: 2.2.104</summary>
    public string? DotNetVersion { get; set; }

    /// <summary>
    ///     Optional package source for which to set up authentication. Will consult any existing NuGet.config in the root of the repo and provide a temporary
    ///     NuGet.config using the NUGET_AUTH_TOKEN environment variable as a ClearTextPassword
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    ///     Optional OWNER for using packages from GitHub Package Registry organizations/users other than the current repository's owner. Only used if a GPR URL is
    ///     also provided in source-url
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    ///     Optional NuGet.config location, if your NuGet.config isn't located in the root of the repo.
    /// </summary>
    public string? ConfigFile { get; set; }

    /// <summary>
    ///     The nuget auth token (NUGET_AUTH_TOKEN)
    /// </summary>
    /// <remarks>
    ///     Defaults to ${{secrets.GITHUB_TOKEN}} if the source url is given
    /// </remarks>
    public string? NuGetAuthToken { get; set; }

    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        // WithProperties(x => x.Underscore().Dasherize().ToLowerInvariant());
        if (!string.IsNullOrWhiteSpace(DotNetVersion))
        {
            With.Add("dotnet-version", DotNetVersion);
        }

        if (!string.IsNullOrWhiteSpace(SourceUrl))
        {
            With.Add("source-url", SourceUrl);
        }

        if (!string.IsNullOrWhiteSpace(Owner))
        {
            With.Add("owner", Owner);
        }

        if (!string.IsNullOrWhiteSpace(SourceUrl))
        {
            NuGetAuthToken = string.IsNullOrWhiteSpace(NuGetAuthToken) ? "${{ secrets.GITHUB_TOKEN }}" : NuGetAuthToken;
        }

        if (!string.IsNullOrWhiteSpace(NuGetAuthToken))
        {
            Environment.Add("NUGET_AUTH_TOKEN", NuGetAuthToken);
        }

        base.Write(writer);
    }
}

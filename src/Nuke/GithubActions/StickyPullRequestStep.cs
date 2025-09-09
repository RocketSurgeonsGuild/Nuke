using System.Runtime.Serialization;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A wrapper around the Sticky Pull Request Comment step
/// </summary>
[PublicAPI]
public class StickyPullRequestStep : UsingStep
{
    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        WithProperties(x => x.Kebaberize());
        ( (IDictionary<string, string>)With ).AddIfMissing("GITHUB_TOKEN", GithubToken ?? "${{ secrets.GITHUB_TOKEN }}");
        base.Write(writer);
    }

    /// <summary>Indicate if new comment messages should be appended to previous comment message</summary>
    public bool? Append { get; set; }

    /// <summary>Delete the previously created comment</summary>
    public bool? Delete { get; set; }

    /// <summary>Indicates whether to follow symbolic links for path</summary>
    public bool? FollowSymbolicLinks { get; set; }

    /// <summary>
    ///     The github token, defaults to use the secret if not provided
    /// </summary>
    [IgnoreDataMember]
    public string? GithubToken { get; set; }

    /// <summary>The header to determine if the comment is to be updated</summary>
    public string? Header { get; set; }

    /// <summary>Hide previously created comment</summary>
    public bool? Hide { get; set; }

    /// <summary>Hide previous comment before creating a new comment</summary>
    public bool? HideAndRecreate { get; set; }

    /// <summary>The reasons a piece of content can be reported or minimized</summary>
    public string? HideClassify { get; set; }

    /// <summary>Hide summary tags in the previously created comment</summary>
    public bool? HideDetails { get; set; }

    /// <summary>Indicates whether to ignore missing or empty messages</summary>
    public bool? IgnoreEmpty { get; set; }

    /// <summary>Comment message</summary>
    public string? Message { get; set; }

    /// <summary>Pull request number for push event</summary>
    public int? Number { get; set; }

    /// <summary>Only create a new comment if there is no existing one</summary>
    public bool? OnlyCreate { get; set; }

    /// <summary>Only update an existing comment if there is one</summary>
    public bool? OnlyUpdate { get; set; }

    /// <summary>Another repo owner</summary>
    public string? Owner { get; set; }

    /// <summary>Glob path to file(s) containing comment message</summary>
    public string? Path { get; set; }

    /// <summary>Indicate if previous comment should be removed before creating a new comment</summary>
    public bool? Recreate { get; set; }

    /// <summary>Another repo name limited use on GitHub enterprise</summary>
    public string? Repo { get; set; }

    /// <summary>Only update or recreate if message is different from previous</summary>
    public bool? SkipUnchanged { get; set; }

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="name"></param>
    public StickyPullRequestStep(string name) : base(name) => Uses = "marocchino/sticky-pull-request-comment@v2";
}

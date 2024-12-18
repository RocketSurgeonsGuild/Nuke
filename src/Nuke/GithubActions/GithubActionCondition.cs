#pragma warning disable CA2225
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Defines an action condition
/// </summary>
/// <remarks>
///     The default constructor
/// </remarks>
/// <param name="condition"></param>
[PublicAPI]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class GithubActionCondition(string condition)
{
    /// <summary>
    ///     Convert the condition expression to a string.
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static implicit operator string(GithubActionCondition condition) => condition.Condition;

    /// <summary>
    ///     Convert an expression string into a GithubActionCondition
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static implicit operator GithubActionCondition(string condition) => new(condition);

    /// <summary>
    ///     The success condition
    /// </summary>
    public static GithubActionCondition Success { get; } = new("success()");

    /// <summary>
    ///     The always condition
    /// </summary>
    public static GithubActionCondition Always { get; } = new("always()");

    /// <summary>
    ///     The cancelled condition
    /// </summary>
    public static GithubActionCondition Cancelled { get; } = new("cancelled()");

    /// <summary>
    ///     The failure condition
    /// </summary>
    public static GithubActionCondition Failure { get; } = new("failure()");

    /// <summary>
    ///     The condition expression
    /// </summary>
    public string Condition { get; } = condition;

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    /// <inheritdoc />
    public override string ToString() => Condition;
}

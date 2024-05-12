using System.Collections.ObjectModel;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReportGenerator;

namespace Rocket.Surgery.Nuke;

#pragma warning disable CA1724
/// <summary>
///     Extension methods for working with nuke build tasks
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Convert a given build into it's implementation interface
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T As<T>(this T value) where T : notnull
    {
        return value;
    }

    /// <summary>
    ///     Convert a given build into it's implementation interface
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CastAs<T>(this object value) where T : notnull
    {
        return (T)value;
    }

    // ReSharper disable once CommentTypo
    /// <summary>
    ///     A method that ensures the given directory exists or is cleaned
    /// </summary>
    /// <param name="target"></param>
    /// <param name="testResultsDirectory"></param>
    /// <returns></returns>
    public static ITargetDefinition CreateOrCleanDirectory(this ITargetDefinition target, AbsolutePath testResultsDirectory)
    {
        return target.Executes(testResultsDirectory.CreateOrCleanDirectory);
    }

    /// <summary>
    ///     <p>
    ///         <em>Sets <see cref="ReportGeneratorSettings.Reports" /> to a new list</em>
    ///     </p>
    ///     <p>The coverage reports that should be parsed (separated by semicolon). Wildcards are allowed.</p>
    /// </summary>
    [Pure]
    public static T SetReports<T>(this T toolSettings, IEnumerable<AbsolutePath> reports) where T : ReportGeneratorSettings
    {
        return toolSettings.SetReports(reports.Select(z => z.ToString()).ToArray());
    }

    /// <summary>
    ///     Determine if there is a pullrequest happening or not.
    /// </summary>
    /// <param name="actions"></param>
    /// <returns></returns>
    public static bool IsPullRequest(this GitHubActions? actions)
    {
        return actions?.EventName is "pull_request" or "pull_request_target";
    }

    /// <summary>
    ///     Add a value to the dictionary if it's missing
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> AddIfMissing<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out _)) return dictionary;
        dictionary[key] = value;
        return dictionary;
    }

    /// <summary>
    ///     Add a value to the dictionary if it's missing
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IReadOnlyDictionary<TKey, TValue> AddIfMissing<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out _)) return dictionary;

        var newDictionary = dictionary.ToDictionary(z => z.Key, z => z.Value);
        newDictionary[key] = value;
        return new ReadOnlyDictionary<TKey, TValue>(newDictionary);
    }

    /// <summary>
    ///     Add a value to the dictionary if it's missing and replace it if it's set
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ReplaceIfSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value) where TKey : notnull
    {
        dictionary[key] = value;
        return dictionary;
    }

    /// <summary>
    ///     Add a value to the dictionary if it's missing and replace it if it's set
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IReadOnlyDictionary<TKey, TValue> ReplaceIfSet<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        var newDictionary = dictionary.ToDictionary(z => z.Key, z => z.Value);
        newDictionary[key] = value;
        return new ReadOnlyDictionary<TKey, TValue>(newDictionary);
    }
}
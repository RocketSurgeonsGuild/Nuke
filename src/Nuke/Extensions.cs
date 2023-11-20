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
    /// A method that ensures the given directory exists or is cleaned
    /// </summary>
    /// <param name="target"></param>
    /// <param name="testResultsDirectory"></param>
    /// <returns></returns>
    public static ITargetDefinition CreateOrCleanDirectory(this ITargetDefinition target, AbsolutePath testResultsDirectory)
    {
        return target.Executes(testResultsDirectory.CreateOrCleanDirectory);
    }

    /// <summary>
    ///   <p><em>Sets <see cref="ReportGeneratorSettings.Reports"/> to a new list</em></p>
    ///   <p>The coverage reports that should be parsed (separated by semicolon). Wildcards are allowed.</p>
    /// </summary>
    [Pure]
    public static T SetReports<T>(this T toolSettings, IEnumerable<AbsolutePath> reports) where T : ReportGeneratorSettings
    {
        return toolSettings.SetReports(reports.Select(z => z.ToString()).ToArray());
    }
}

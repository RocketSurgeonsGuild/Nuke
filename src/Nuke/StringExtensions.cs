using System.Text.RegularExpressions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Extensions for strings
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     Removes alpha characters from a string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string with no alpha characters.</returns>
    public static string RemoveAlphaCharacters(this string input) => MyRegex().Replace(input, "");
    [GeneratedRegex("[^0-9.]")]
    private static partial Regex MyRegex();
}

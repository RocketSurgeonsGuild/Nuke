using System.Text.RegularExpressions;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Extensions for strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Removes alpha characters from a string.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The string with no alpha characters.</returns>
    public static string RemoveAlphaCharacters(this string input) => Regex.Replace(input, "[^0-9.]", "");
}

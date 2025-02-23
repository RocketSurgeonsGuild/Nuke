using Nuke.Common.Utilities.Collections;

namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     Custom file writer extensions to help with writing out string dictionaries and multi line strings.
/// </summary>
public static class CustomFileWriterExtensions
{
    /// <summary>
    ///     Write a set of key value pairs
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="key"></param>
    /// <param name="dictionary"></param>
    public static void WriteKeyValues(this CustomFileWriter writer, string key, IDictionary<string, string> dictionary)
    {
        if (!dictionary.Any()) return;

        writer.WriteLine(key + ":");
        using (writer.Indent())
        {
            foreach (var z in dictionary)
            {
                WriteValue(writer, z);
            }
        }
    }

    internal static void WriteValue(this CustomFileWriter writer, KeyValuePair<string, string> kvp)
    {
        ( var key, var value ) = kvp;
        if (value.StartsWith('>') || value.StartsWith('|'))
        {
            var values = value.Split('\n');
            writer.WriteLine($"{key}: {values[0].TrimEnd()}");
            using (writer.Indent())
            {
                foreach (var v in values.Skip(1))
                {
                    writer.WriteLine(v.Trim());
                }
            }

            return;
        }

        writer.WriteLine(value.Contains('\'', StringComparison.Ordinal) ? $"{key}: \"{value}\"" : $"{key}: '{value}'");
    }
}

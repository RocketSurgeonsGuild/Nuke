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
        if (dictionary.Any())
        {
            writer.WriteLine(key + ":");
            using (writer.Indent())
            {
                dictionary.ForEach(z => WriteValue(writer, z));
            }
        }
    }

    private static void WriteValue(CustomFileWriter writer, KeyValuePair<string, string> kvp)
    {
        var (key, value) = kvp;
        if (value.StartsWith(">", StringComparison.Ordinal) || value.StartsWith("|", StringComparison.Ordinal))
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

        if (value.Contains('\'', StringComparison.Ordinal))
        {
            writer.WriteLine($"{key}: \"{value}\"");
        }
        else
        {
            writer.WriteLine($"{key}: '{value}'");
        }
    }
}

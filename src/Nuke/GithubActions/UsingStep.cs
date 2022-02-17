using System.Reflection;
using Nuke.Common.Utilities.Collections;

// ReSharper disable MemberCanBeProtected.Global
#pragma warning disable CA1308
#pragma warning disable CA2227
namespace Rocket.Surgery.Nuke.GithubActions;

/// <summary>
///     A step that runs a given action
/// </summary>
public class UsingStep : BaseGitHubActionsStep
{
    /// <summary>
    ///     The constructor with the display name
    /// </summary>
    /// <param name="name"></param>
    public UsingStep(string name) : base(name)
    {
    }

    /// <summary>
    ///     The action to use.
    /// </summary>
    public string? Uses { get; set; }

    /// <summary>
    ///     The properties to use with the action
    /// </summary>
    public Dictionary<string, string> With { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Defines the properties to include with the step
    /// </summary>
    /// <param name="transformName"></param>
    protected void WithProperties(Func<string, string> transformName)
    {
        foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                          .Where(z => z.CanRead && z.CanWrite && z.DeclaringType == GetType()))
        {
            var value = property.GetValue(this);
            if (value == null) continue;

            With?.Add(
                transformName(property.Name), value switch
                {
                    null     => "",
                    bool b   => b.ToString().ToLowerInvariant(),
                    string s => s,
                    _        => value.ToString() ?? ""
                }
            );
        }
    }


    /// <inheritdoc />
    public override void Write(CustomFileWriter writer)
    {
        base.Write(writer);

        using (writer.Indent())
        {
            writer.WriteLine($"uses: {Uses}");
            writer.WriteKeyValues("with", With);
        }
    }
}

public static class CustomFileWriterExtensions
{
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

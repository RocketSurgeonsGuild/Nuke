using System.Reflection;
using System.Runtime.Serialization;

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
    public UsingStep(string name) : base(name) { }

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
        foreach (var property in GetType()
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(z => z is { CanRead: true, CanWrite: true } && z.DeclaringType == GetType())
                                .Where(z => z.GetCustomAttribute<IgnoreDataMemberAttribute>() is null)
                )
        {
            var value = property.GetValue(this);
            if (value == null) continue;

            With.TryAdd(
                transformName(property.Name),
                value switch { null => string.Empty, bool b => b.ToString().ToLowerInvariant(), string s => s, _ => value.ToString() ?? "", }
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

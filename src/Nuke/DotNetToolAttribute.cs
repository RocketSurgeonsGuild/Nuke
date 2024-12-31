using System.Reflection;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     This property loads the given dotnet global tool.
/// </summary>
/// <remarks>
///     DotNetToolAttribute constructor
/// </remarks>
/// <param name="command"></param>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DotNetToolAttribute(string command) : ToolInjectionAttributeBase
{
    private readonly string _command = command;

    /// <inheritdoc />
    public override ToolRequirement GetRequirement(MemberInfo member) => new();

    /// <inheritdoc />
    public override object GetValue(MemberInfo member, object instance) => DotNetTool.GetTool(_command);

    #pragma warning disable CA1019
    #pragma warning restore CA1019
}

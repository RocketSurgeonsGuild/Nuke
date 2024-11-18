using System.Reflection;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     This property loads the given dotnet global tool.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DotNetToolAttribute : ToolInjectionAttributeBase
{
    private readonly string _command;

    /// <summary>
    ///     DotNetToolAttribute constructor
    /// </summary>
    /// <param name="command"></param>
    #pragma warning disable CA1019
    public DotNetToolAttribute(string command)
    {
        _command = command;
    }
    #pragma warning restore CA1019

    /// <inheritdoc />
    public override ToolRequirement GetRequirement(MemberInfo member) => new();

    /// <inheritdoc />
    public override object GetValue(MemberInfo member, object instance) => DotNetTool.GetTool(_command);
}

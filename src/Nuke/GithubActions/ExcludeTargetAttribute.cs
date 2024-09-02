#pragma warning disable CA1851
// ReSharper disable PossibleMultipleEnumeration
namespace Rocket.Surgery.Nuke.GithubActions;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ExcludeTargetAttribute : Attribute
{

}

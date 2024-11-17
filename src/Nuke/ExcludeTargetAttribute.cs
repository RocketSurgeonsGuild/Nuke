#pragma warning disable CA1851
// ReSharper disable PossibleMultipleEnumeration
namespace Rocket.Surgery.Nuke;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ExcludeTargetAttribute : Attribute { }

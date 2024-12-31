using System.ComponentModel;
using Nuke.Common.Tooling;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Target platform the application will build for.
/// </summary>
[PublicAPI]
[Serializable]
[TypeConverter(typeof(TypeConverter<TargetPlatform>))]
public class TargetPlatform : Enumeration
{
    /// <inheritdoc />
    public override string ToString() => this;

    /// <summary>
    ///     Performs an implicit conversion from <see cref="XamarinConfiguration" /> to <see cref="string" />.
    /// </summary>
    /// <param name="platform">The target platform.</param>
    /// <returns>
    ///     The result of the conversion.
    /// </returns>
    public static implicit operator string(TargetPlatform platform) => platform.Value;

    /// <summary>
    ///     Any CPU Target
    /// </summary>
    public static readonly TargetPlatform AnyCPU = new() { Value = "Any CPU" };

    /// <summary>
    ///     iPhone Target
    /// </summary>
    public static readonly TargetPlatform iPhone = new() { Value = nameof(iPhone) };

    /// <summary>
    ///     iPhone Simulator Target
    /// </summary>
    public static readonly TargetPlatform iPhoneSimulator = new() { Value = nameof(iPhoneSimulator) };
}

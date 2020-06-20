using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Nuke.Common.Tooling;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Target platform the application will build for.
    /// </summary>
    [PublicAPI]
    [Serializable]
    [TypeConverter(typeof(TypeConverter<TargetPlatform>))]
    public class TargetPlatform : Enumeration
    {
        /// <summary>
        /// iPhone Target
        /// </summary>
        public static readonly TargetPlatform iPhone = new TargetPlatform { Value = nameof(iPhone) };

        /// <summary>
        /// iPhone Simulator Target
        /// </summary>
        public static readonly TargetPlatform iPhoneSimulator = new TargetPlatform { Value = nameof(iPhoneSimulator) };

        /// <summary>
        /// Any CPU Target
        /// </summary>
        public static readonly TargetPlatform AnyCPU = new TargetPlatform { Value = "Any CPU" };

        /// <summary>
        /// Performs an implicit conversion from <see cref="XamarinConfiguration" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="platform">The target platform.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(TargetPlatform platform) => platform.Value;

        /// <inheritdoc />
        public override string ToString() => this;
    }
}
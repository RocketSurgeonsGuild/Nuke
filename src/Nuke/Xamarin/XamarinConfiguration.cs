using System.ComponentModel;
using Nuke.Common.Tooling;

#pragma warning disable CA2211 // Non-constant fields should not be visible

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Represents xamarin build configuration.
    /// </summary>
    [TypeConverter(typeof(TypeConverter<XamarinConfiguration>))]
    public class XamarinConfiguration : Enumeration
    {
        /// <summary>
        /// The debug mock
        /// </summary>
        public static XamarinConfiguration DebugMock =
            new XamarinConfiguration { Value = nameof(DebugMock) };

        /// <summary>
        /// The debug dev
        /// </summary>
        public static XamarinConfiguration DebugDev =
            new XamarinConfiguration { Value = nameof(DebugDev) };

        /// <summary>
        /// The debug test
        /// </summary>
        public static XamarinConfiguration DebugTest =
            new XamarinConfiguration { Value = nameof(DebugTest) };

        /// <summary>
        /// The mock
        /// </summary>
        public static XamarinConfiguration Mock =
            new XamarinConfiguration { Value = nameof(Mock) };

        /// <summary>
        /// The dev
        /// </summary>
        public static XamarinConfiguration Dev =
            new XamarinConfiguration { Value = nameof(Dev) };

        /// <summary>
        /// The test
        /// </summary>
        public static XamarinConfiguration Test =
            new XamarinConfiguration { Value = nameof(Test) };

        /// <summary>
        /// The store
        /// </summary>
        public static XamarinConfiguration Store =
            new XamarinConfiguration { Value = nameof(Store) };

        /// <summary>
        /// Performs an implicit conversion from <see cref="XamarinConfiguration" /> to <see cref="System.String" />.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(XamarinConfiguration configuration) => configuration.Value;

        /// <inheritdoc />
        public override string ToString() => this;
    }
}
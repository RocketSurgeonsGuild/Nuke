using Nuke.Common.Tooling;
using Serilog;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a common property for build configuration
/// </summary>
public interface IHaveConfiguration : IHave
{
    /// <summary>
    ///     The build configuration
    /// </summary>
    string Configuration { get; }
}

/// <summary>
///     Defines the configuration as strongly typed enumeration
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHaveConfiguration<out T> : IHaveConfiguration
    where T : Enumeration
{
    /// <summary>
    ///     The build configuration
    /// </summary>
    public new T Configuration { get; }

    string IHaveConfiguration.Configuration => Configuration.ToString();
}

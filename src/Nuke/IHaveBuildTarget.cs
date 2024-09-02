namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines the build target
/// </summary>
public interface IHaveBuildTarget : IHave
{
    /// <summary>
    ///     The Build Target
    /// </summary>
    Target Build { get; }
}


/// <summary>
///     Defines the build target
/// </summary>
public interface IHaveGenerateDocumentationTarget : IHave
{
    /// <summary>
    ///     The Build Target
    /// </summary>
    Target GenerateDocumentation { get; }
}


using Rocket.Surgery.Nuke.DotNetCore;

namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a target that cleans common directories
/// </summary>
public interface IHaveCommonLintTargets :
    ICanDotNetFormat,
    ICanPrettier,
    IHavePublicApis,
    ICanUpdateReadme,
    ICanUpdateSolution,
    ICanRegenerateBuildConfiguration;
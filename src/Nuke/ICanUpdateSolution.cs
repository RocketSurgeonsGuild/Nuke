namespace Rocket.Surgery.Nuke;

/// <summary>
///     A tool to ensure the solution is updated with relevant files that exist on disk but not in projects.
/// </summary>
[PublicAPI]
public interface ICanUpdateSolution : IHaveSolution
{
    /// <summary>
    ///     The solution updater that ensures that all the files are in the solution.
    /// </summary>
    public Target GenerateSolutionItems
        => d => d
               .Unlisted()
               .TryTriggeredBy<ICanLint>(z => z.PostLint)
               .TryAfter<ICanLint>(z => z.PostLint)
               .Executes(
                    () =>
                    {
                        TargetAttributeCache.BuildCache();
                        var attributes = GetType()
                                        .GetCustomAttributes(true)
                                        .OfType<SolutionUpdaterConfigurationAttribute>()
                                        .ToArray();
                        SolutionUpdater.UpdateSolution(
                            Solution,
                            attributes.SelectMany(z => z.AdditionalRelativeFolderFilePatterns),
                            attributes.SelectMany(z => z.AdditionalConfigFolderFilePatterns),
                            attributes.SelectMany(z => z.AdditionalIgnoreFolderFilePatterns)
                        );
                    }
                );
}

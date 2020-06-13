namespace Rocket.Surgery.Nuke
{
    /// <summary>
    /// Defines ownership of a responsibility.
    /// </summary>
    public interface IHave
    {
    }

    /// <summary>
    /// Defines an action in that responds is initiated by another action.
    /// </summary>
    public interface ITrigger
    {
    }

    /// <summary>
    /// Defines an artifact.
    /// </summary>
    public interface IGenerate
    {
    }

    /// <summary>
    /// Defines an action on a build.
    /// </summary>
    public interface ICan
    {
    }

    /// <summary>
    /// Defines understanding of the state of a repository.
    /// </summary>
    public interface IComprehend
    {
    }
}
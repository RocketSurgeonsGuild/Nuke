namespace Rocket.Surgery.Nuke;

/// <summary>
///     Extension methods for working with nuke build tasks
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Convert a given build into it's implementation interface
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T As<T>(this T value) where T : INukeBuild
    {
        return value;
    }
}

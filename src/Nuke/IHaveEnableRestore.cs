namespace Rocket.Surgery.Nuke;

/// <summary>
///     Defines a value whether to enable restore.
/// </summary>
public interface IHaveEnableRestore : IHave
{
    /// <summary>
    ///     A value indicating whether to enable restoring for a given target
    /// </summary>
    [Parameter("A value indicating whether to enable restoring for a given target", Name = "EnableRestore")]
    public bool EnableRestore => EnvironmentInfo.GetVariable<bool?>("EnableRestore")
     ?? TryGetValue<bool?>(() => EnableRestore)
     ?? false;
}

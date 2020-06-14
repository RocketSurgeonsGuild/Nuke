using Nuke.Common;

#pragma warning disable 1591


namespace Rocket.Surgery.Nuke.Xamarin
{
    public interface IHaveiOSTargetPlatform : IHave
    {
        /// <summary>
        /// Gets the target platform.
        /// </summary>
        /// <value>The target platform.</value>
        [Parameter("The target platform for iOS")]
        public TargetPlatform iOSTargetPlatform { get; }
    }
}
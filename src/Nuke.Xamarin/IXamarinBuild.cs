using System;
using System.Linq.Expressions;
using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Base build plan for Xamarin based applications
    /// </summary>
    public interface IXamarinBuild : IRocketBoosterBuild
    {
        /// <summary>
        /// Configuration to build - Default is 'DebugMock' (local) or 'Mock' (release)
        /// </summary>
        XamarinConfiguration Configuration { get; }

        /// <summary>
        /// A value indicated whether the build host is OSX.
        /// </summary>
        Expression<Func<bool>> IsOsx { get; set; }

        /// <summary>
        /// nuget restore
        /// </summary>
        Target Restore { get; }

        /// <summary>
        /// msbuild
        /// </summary>
        Target Build { get; }

        /// <summary>
        /// xunit test
        /// </summary>
        Target Test { get; }

        /// <summary>
        /// package the application
        /// </summary>
        Target Package { get; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public class AndroidBuild : XamarinBuild
    {
        public Target XamarinAndroid => _ => _;
    }
}

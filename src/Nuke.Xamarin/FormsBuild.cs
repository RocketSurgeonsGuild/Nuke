using System;
using System.Collections.Generic;
using System.Text;
using Nuke.Common;

namespace Rocket.Surgery.Nuke.Xamarin
{
    public class FormsBuild : XamarinBuild
    {
        public Target XamarinForms => _ => _;
    }
}

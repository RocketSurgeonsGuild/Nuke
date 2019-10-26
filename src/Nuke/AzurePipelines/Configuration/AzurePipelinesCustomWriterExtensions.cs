using System;
using System.Linq;
using Nuke.Common.Utilities;

namespace Rocket.Surgery.Nuke.AzurePipelines.Configuration
{
    public static class AzurePipelinesCustomWriterExtensions
    {
        public static IDisposable WriteBlock(this CustomFileWriter writer, string text)
        {
            return DelegateDisposable
                .CreateBracket(() => writer.WriteLine(text))
                .CombineWith(writer.Indent());
        }
    }
}
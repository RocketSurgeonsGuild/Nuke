using System.Xml.Serialization;
using Schemas.VisualStudio.TeamTest;

namespace Rocket.Surgery.Nuke.Temp.LiquidReporter.Loaders;

internal static class TrxLoader
{
    internal static TestRunType FromFile(string file)
    {
        var ser = new XmlSerializer(typeof(TestRunType));
        using (var reader = new StreamReader(file))
        {
#pragma warning disable CA5369
            if (ser.Deserialize(reader) is TestRunType results)
#pragma warning restore CA5369
                return results;
        }

        throw new InvalidDataException($"Provided file {file} could not be deserialised, check file is valid TRX XML");
    }
}

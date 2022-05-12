using System.Xml.Serialization;
using Schemas.VisualStudio.TeamTest;

namespace Rocket.Surgery.Nuke.Temp.LiquidReporter.Loaders;

internal class TrxLoader
{
    internal static TestRunType FromFile(string file)
    {
        var ser = new XmlSerializer(typeof(TestRunType));
        using (var reader = new StreamReader(file))
        {
            if (ser.Deserialize(reader) is TestRunType results)
                return results;
        }

        throw new InvalidDataException($"Provided file {file} could not be deserialised, check file is valid TRX XML");
    }
}

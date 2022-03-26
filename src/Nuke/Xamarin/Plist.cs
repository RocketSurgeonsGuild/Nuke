using System.Collections;
using System.Globalization;
using System.Xml.Linq;
using Nuke.Common.IO;
using Serilog;

namespace Rocket.Surgery.Nuke.Xamarin;

/// <summary>
///     Taken from https://github.com/cake-contrib/Cake.Plist/blob/develop/src/Cake.Plist/PlistConverter.cs.
/// </summary>
internal static class Plist
{
    /// <summary>
    ///     Deserializes the .plist file provided.
    /// </summary>
    /// <param name="plist">The plist file.</param>
    /// <returns>The deserialized plist.</returns>
    public static dynamic Deserialize(AbsolutePath plist)
    {
        using var stream = File.OpenRead(plist);
        var document = XDocument.Load(stream);

        return DeserializeXml(document.Root!);
    }

    /// <summary>
    ///     Serializes the object provided to the .plist file.
    /// </summary>
    /// <param name="path">The path to the plist.</param>
    /// <param name="value">The object to serialize.</param>
    public static void Serialize(AbsolutePath path, object value)
    {
        SerializeDocument(value)
           .Save(path, SaveOptions.OmitDuplicateNamespaces);
    }

    /// <summary>
    ///     Serializes the .plist file provided.
    /// </summary>
    /// <param name="item">The plist object.</param>
    /// <returns>The xml document.</returns>
    private static XDocument SerializeDocument(object item)
    {
        var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
        doc.AddFirst(
            new XDocumentType(
                "plist",
                "-//Apple//DTD PLIST 1.0//EN",
                "http://www.apple.com/DTDs/PropertyList-1.0.dtd",
                string.Empty
            )
        );

        var plist = new XElement("plist");
        plist.SetAttributeValue("version", "1.0");
        plist.Add(SerializeObject(item));
        doc.Add(plist);

        return doc;
    }

    /// <summary>
    ///     Serializes the .plist file provided.
    /// </summary>
    /// <param name="item">The plist object.</param>
    /// <returns>The xml element.</returns>
    private static XElement? SerializeObject(object item)
    {
        switch (item)
        {
            case string:
                Log.Verbose("string: {String}", item);
                return new XElement("string", item);
            case double:
            case float:
            case decimal:
                Log.Verbose("floating point: {Float}", item);
                return new XElement("real", Convert.ToString(item, CultureInfo.InvariantCulture));
            case int:
            case long:
                Log.Verbose("integer: {Integer}", item);
                return new XElement("integer", Convert.ToString(item, CultureInfo.InvariantCulture));
            case bool when item as bool? == true:
                Log.Verbose("boolean: {Boolean}", item);
                return new XElement("true");
            case bool when item as bool? == false:
                Log.Verbose("boolean: {Boolean}", item);
                return new XElement("false");
            case DateTime time:
                Log.Verbose("DateTime: {DateTime}", item);
                return new XElement("date", time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeFormatInfo.InvariantInfo));
            case DateTimeOffset offset:
                Log.Verbose("DateTimeOffset: {DateTimeOffset}", item);
                return new XElement("date", offset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", DateTimeFormatInfo.InvariantInfo));
            case byte[] bytes:
                Log.Verbose("DateTimeOffset: {DateTimeOffset}", item);
                return new XElement("data", Convert.ToBase64String(bytes));
            case IDictionary dictionary:
            {
                var dict = new XElement("dict");

                var enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    dict.Add(new XElement("key", enumerator.Key));
                    dict.Add(SerializeObject(enumerator.Value!));
                }

                Log.Verbose("Dictionary: {Dictionary}", item);
                return dict;
            }

            case IEnumerable enumerable:
            {
                var array = new XElement("array");

                foreach (var itm in enumerable)
                {
                    array.Add(SerializeObject(itm!));
                }

                Log.Verbose("Array: {Array}", item);
                return array;
            }

            default:
                return null;
        }
    }

    private static dynamic DeserializeXml(XElement element)
    {
        switch (element.Name.LocalName)
        {
            case "plist":
                return DeserializeXml(element.Elements().First());
            case "string":
                return element.Value;
            case "real":
                return double.Parse(element.Value, CultureInfo.InvariantCulture);
            case "integer":
                return int.Parse(element.Value, CultureInfo.InvariantCulture);
            case "true":
                return true;
            case "false":
                return false;
            case "date":
                return DateTime.Parse(element.Value, CultureInfo.InvariantCulture);
            case "data":
                return Convert.FromBase64String(element.Value);
            case "array":
            {
                if (!element.HasElements)
                {
                    return Array.Empty<object>();
                }

                var rawArray = element.Elements().Select(DeserializeXml).ToArray();

                var type = rawArray[0].GetType();
                if (rawArray.Any(val => val.GetType() != type))
                {
                    return rawArray;
                }

                var typedArray = Array.CreateInstance(type, rawArray.Length);
                rawArray.CopyTo(typedArray, 0);

                return typedArray;
            }

            case "dict":
            {
                var dictionary = new Dictionary<string, object>();

                var inner = element.Elements().ToArray();

                for (var idx = 0; idx < inner.Length; idx++)
                {
                    var key = inner[idx];
                    if (key.Name.LocalName != "key")
                    {
#pragma warning disable CA2201
                        throw new Exception("Even items need to be keys");
#pragma warning restore CA2201
                    }

                    idx++;
                    dictionary[key.Value] = DeserializeXml(inner[idx]);
                }

                return dictionary;
            }

            default:
                return null!;
        }
    }
}

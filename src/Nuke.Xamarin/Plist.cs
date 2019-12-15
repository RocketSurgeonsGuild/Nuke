using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Array = System.Array;
using Convert = System.Convert;
using DateTime = System.DateTime;
using static Nuke.Common.IO.PathConstruction;

namespace Rocket.Surgery.Nuke.Xamarin
{
    /// <summary>
    /// Taken from https://github.com/cake-contrib/Cake.Plist/blob/develop/src/Cake.Plist/PlistConverter.cs
    /// </summary>
    internal class Plist
    {
        /// <summary>
        /// Deserializes the .plist file provided.
        /// </summary>
        /// <param name="plist">The plist file.</param>
        /// <returns>The deserialized plist.</returns>
        public static dynamic Deserialize(AbsolutePath plist)
        {
            using (var stream = File.OpenRead(plist))
            {
                var document = XDocument.Load(stream);

                return DeserializeXml(document.Root);
            }
        }

        /// <summary>
        /// Serializes the object provided to the .plist file.
        /// </summary>
        /// <param name="path">The path to the plist.</param>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The deserialized plist.</returns>
        public static void Serialize(AbsolutePath path, object value)
        {
            var doc = SerializeDocument(value);

            string result;

            using (var sw = new MemoryStream())
            {
                using (var strw = new StreamWriter(sw))
                {
                    doc.Save(strw);
                    result = new UTF8Encoding(false).GetString(sw.ToArray());
                }
            }

            using (var stream = File.OpenWrite(path))
            {
                using (var write = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
                {
                    write.Write(result);
                }
            }
        }

        /// <summary>
        /// Serializes the .plist file provided.
        /// </summary>
        /// <param name="item">The plist object</param>
        /// <returns>The xml document</returns>
        private static XDocument SerializeDocument(object item)
        {
            var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            doc.AddFirst(new XDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null));

            var plist = new XElement("plist");
            plist.SetAttributeValue("version", "1.0");
            plist.Add(SerializeObject(item));
            doc.Add(plist);

            return doc;
        }

        /// <summary>
        /// Serializes the .plist file provided.
        /// </summary>
        /// <param name="item">The plist object</param>
        /// <returns>The xml element</returns>
        private static XElement SerializeObject(object item)
        {
            if (item is string)
            {
                return new XElement("string", item);
            }

            if (item is double || item is float || item is decimal)
            {
                return new XElement("real", Convert.ToString(item, CultureInfo.InvariantCulture));
            }

            if (item is int || item is long)
            {
                return new XElement("integer", Convert.ToString(item, CultureInfo.InvariantCulture));
            }

            if (item is bool && (item as bool?) == true)
            {
                return new XElement("true");
            }

            if (item is bool && (item as bool?) == false)
            {
                return new XElement("false");
            }

            if (item is DateTime)
            {
                return new XElement("date", ((DateTime) item).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }

            if (item is DateTimeOffset)
            {
                return new XElement("date", ((DateTimeOffset)item).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            }

            var bytes = item as byte[];
            if (bytes != null)
            {
                return new XElement("data", Convert.ToBase64String(bytes));
            }

            var dictionary = item as IDictionary;
            if (dictionary != null)
            {
                var dict = new XElement("dict");

                var enumerator = dictionary.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    dict.Add(new XElement("key", enumerator.Key));
                    dict.Add(SerializeObject(enumerator.Value));
                }

                return dict;
            }

            var enumerable = item as IEnumerable;
            if (enumerable != null)
            {
                var array = new XElement("array");

                foreach (var itm in enumerable)
                {
                    array.Add(SerializeObject(itm));
                }

                return array;
            }

            return null;
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
                        return rawArray;

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
                            throw new Exception("Even items need to be keys");
                        }

                        idx++;
                        dictionary[key.Value] = DeserializeXml(inner[idx]);
                    }

                    return dictionary;
                }
                default:
                    return null;
            }
        }
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CoptLib.IO;
using CoptLib.Models;

namespace CopticChanter.WebApi.Core.Responses;

[Serializable]
public record Layout(string SessionKey, List<List<IDefinition>> Rows)
{
    public bool TryGetDoc([NotNullWhen(true)] out Doc? doc)
    {

        var firstDef = Rows.FirstOrDefault()?.FirstOrDefault();
        if (firstDef is not Doc foundDoc)
        {
            doc = null;
            return false;
        }

        doc = foundDoc;
        return true;
    }
}

public static class LayoutReaderWriter
{
    public static Layout FromXml(XDocument xml)
    {
        var layoutXml = xml.Root!;
        var sessionKey = layoutXml.Attribute("Session")?.Value
            ?? throw new InvalidDataException("No session key was found.");

        Layout layout = new(sessionKey, new());

        foreach (var rowXml in layoutXml.Elements())
        {
            var row = DocReader.ParseDefinitionCollection(rowXml.Elements(), null);
            layout.Rows.Add(row);
        }

        return layout;
    }

    public static XDocument ToXml(this Layout layout)
    {
        XElement xResponse = new("Layout");
        xResponse.SetAttributeValue("Session", layout.SessionKey);
        foreach (var row in layout.Rows)
        {
            XElement xRow = new("Row");
            foreach (var def in row)
            {
                var cell = DocWriter.SerializeTransformedObject(def);
                xRow.Add(cell);
            }
            xResponse.Add(xRow);
        }

        XDocument xDoc = new();
        xDoc.Add(xResponse);
        return xDoc;
    }

    public static Stream ToXmlString(this Layout layout)
    {
        var xml = layout.ToXml();

        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, Encoding.Unicode);
        using var xmlWriter = XmlWriter.Create(streamWriter);
        xml.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return stream;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public static async Task<Stream> ToXmlStringAsync(this Layout layout, CancellationToken token = default)
    {
        var xml = layout.ToXml();

        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, Encoding.Unicode);

#if NETCOREAPP2_0_OR_GREATER
        await
#endif
        using var xmlWriter = XmlWriter.Create(streamWriter, new XmlWriterSettings
            {
                Async = true,
            });
        await xml.WriteToAsync(xmlWriter, token);
        await xmlWriter.FlushAsync();

        stream.Position = 0;
        return stream;
    }
#endif
}
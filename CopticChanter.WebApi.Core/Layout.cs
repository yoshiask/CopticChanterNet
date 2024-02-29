using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CoptLib.IO;
using CoptLib.Models;

namespace CopticChanter.WebApi.Core;

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
        var sessionKey = layoutXml.Attribute(nameof(Layout.SessionKey))!.Value;
        
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

    public static string ToXmlString(this Layout layout)
    {
        var xml = layout.ToXml();
        
        StringBuilder sb = new("\xFEFF");
        using var xmlWriter = XmlWriter.Create(sb);
        xml.WriteTo(xmlWriter);
        xmlWriter.Flush();

        return sb.ToString();
    }

    #if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public static async Task<string> ToXmlStringAsync(this Layout layout, CancellationToken token = default)
    {
        var xml = layout.ToXml();
        
        StringBuilder sb = new("\xFEFF");
        
        #if NETCOREAPP2_0_OR_GREATER
        await
        #endif
            using var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings
        {
            Async = true,
        });
        await xml.WriteToAsync(xmlWriter, token);
        await xmlWriter.FlushAsync();
        return sb.ToString();
    }
    #endif
}
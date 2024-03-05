using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CoptLib.IO;
using CoptLib.Models;

namespace CopticChanter.WebApi.Core.Responses;

[Serializable]
public record Layout(string SessionKey, string Title, List<List<IDefinition>> Rows)
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
        var title = layoutXml.Attribute("Title")?.Value ?? "";

        Layout layout = new(sessionKey, title, new());

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
        xResponse.SetAttributeValue("Title", layout.Title);
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
}
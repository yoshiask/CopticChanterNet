using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CopticChanter.WebApi.Core;

public static class XmlExtensions
{

    /// <summary>
    /// Writes the XML element to a stream using the UTF-16 encoding.
    /// </summary>
    public static Task<Stream> ToStreamAsync(this XElement xml, CancellationToken token = default)
    {
        XDocument doc = new(xml);
        return doc.ToStreamAsync(token);
    }

    /// <summary>
    /// Writes the XML document to a stream using the UTF-16 encoding.
    /// </summary>
    public static Task<Stream> ToStreamAsync(this XDocument xml, CancellationToken token = default)
    {
        return xml.ToStreamAsync(Encoding.Unicode, token);
    }

    /// <summary>
    /// Writes the XML document to a stream using the specified encoding.
    /// </summary>
    public static async Task<Stream> ToStreamAsync(this XDocument xml, Encoding encoding, CancellationToken token = default)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, encoding);

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
#else
        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, encoding);
        using var xmlWriter = XmlWriter.Create(streamWriter);
        xml.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();
        return stream;
#endif
    }
}
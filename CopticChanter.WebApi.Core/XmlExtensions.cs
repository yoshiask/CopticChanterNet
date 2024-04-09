using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace CopticChanter.WebApi.Core;

public static class XmlExtensions
{
    public static Task<Stream> ToStringAsync(this XElement xml, CancellationToken token = default)
    {
        XDocument doc = new(xml);
        return doc.ToStringAsync(token);
    }
    
    public static async Task<Stream> ToStringAsync(this XDocument xml, CancellationToken token = default)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
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
#else
        MemoryStream stream = new();
        StreamWriter streamWriter = new(stream, Encoding.Unicode);
        using var xmlWriter = XmlWriter.Create(streamWriter);
        xml.WriteTo(xmlWriter);
        await xmlWriter.FlushAsync();
        return stream;
#endif
    }
}
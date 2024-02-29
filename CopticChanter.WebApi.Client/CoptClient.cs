using System.Xml.Linq;
using CopticChanter.WebApi.Core;
using Flurl;
using Flurl.Http;

namespace CopticChanter.WebApi.Client;

public class CoptClient(Url? baseUrl = null)
{
    public const string DefaultBaseUrl = "https://coptic.askharoun.com/api";

    public Url BaseUrl { get; } = baseUrl ?? DefaultBaseUrl;

    public IFlurlRequest GetBase() => new FlurlRequest(BaseUrl);

    public async Task<AvailableContent> GetAvailableContentAsync()
        => await GetBase().AppendPathSegment("content").GetJsonAsync<AvailableContent>();

    public async Task<Layout> GetLayoutAsync(string type, string id, string? sessionKey)
    {
        var response = await GetBase()
            .AppendPathSegments("layout", type, id)
            .SendUrlEncodedAsync(HttpMethod.Get, null!)
            .ReceiveStream();
        
        var xml = XDocument.Load(response);
        return LayoutReaderWriter.FromXml(xml);
    }
}
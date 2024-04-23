using System.Xml.Linq;
using CopticChanter.WebApi.Core;
using CopticChanter.WebApi.Core.Requests;
using CopticChanter.WebApi.Core.Responses;
using CoptLib.Writing;
using CoptLib.Writing.Lexicon;
using Flurl;
using Flurl.Http;

namespace CopticChanter.WebApi.Client;

public class CoptClient(Url? baseUrl = null)
{
    public const string DefaultBaseUrl = "https://coptic.askharoun.com/api";

    public Url BaseUrl { get; } = baseUrl ?? DefaultBaseUrl;

    public IFlurlRequest GetBase() => new FlurlRequest(BaseUrl.Clone());

    public IFlurlRequest GetBase(string? sessionKey) => GetBase().SetQueryParam("sessionKey", sessionKey);

    public async Task<AvailableContent> GetAvailableContentAsync()
        => await GetBase().AppendPathSegment("content").GetJsonAsync<AvailableContent>();

    public async Task<CustomContentResponse> PostCustomContentAsync(string type, Stream stream, string? sessionKey)
    {
        StreamContent content = new(stream);
        content.Headers.ContentType = new(ContentTypes.TypeToMime(type));

        var response = await GetBase(sessionKey)
            .AppendPathSegment("content")
            .PostAsync(content);

        return await response.GetJsonAsync<CustomContentResponse>();
    }

    public async Task<Layout> GetLayoutAsync(string type, string id, string? sessionKey,
        LayoutRequest? options)
    {
        var request = GetBase(sessionKey)
            .AppendPathSegments("layout", type, id);

        var response = await request.PostUrlEncodedAsync(options);
        var xmlStream = await response.GetStreamAsync();
        
        var xml = XDocument.Load(xmlStream);
        return LayoutReaderWriter.FromXml(xml);
    }

    public async Task<LexiconSearchResponse> SearchLexiconAsync(string query, LanguageInfo usage)
    {
        var request = GetBase()
            .AppendPathSegments("lexicon")
            .SetQueryParam("q", query)
            .SetQueryParam("usage", usage.ToString());

        var response = await request.GetStreamAsync();

        var xml = XDocument.Load(response);
        return LexiconSearchResponseReaderWriter.FromXml(xml);
    }

    public async Task<LexiconEntry> GetEntryAsync(string id)
    {
        var request = GetBase()
            .AppendPathSegments("lexicon", id);

        var response = await request.GetStreamAsync();

        var xml = XDocument.Load(response);
        return LexiconEntryReaderWriter.FromXml(xml);
    }

    public async Task<string> TransliterateAsync(TransliterationRequest requestBody)
    {
        var response = await GetBase()
            .AppendPathSegments("linguistics", "transliterate")
            .PostJsonAsync(requestBody);

        return await response.GetStringAsync();
    }

    public async Task<string> TestScriptAsync(string scriptBody, string typeId, string? sessionKey)
    {
        var response = await GetBase(sessionKey)
            .AppendPathSegments("authoring", "script")
            .SetQueryParam("typeId", typeId)
            .PostStringAsync(scriptBody);

        return await response.GetStringAsync();
    }
}
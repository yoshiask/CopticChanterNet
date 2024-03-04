using System.Xml.Linq;
using CopticChanter.WebApi.Core;
using CopticChanter.WebApi.Core.Responses;
using CoptLib.Writing;
using Flurl;
using Flurl.Http;

namespace CopticChanter.WebApi.Client;

public class CoptClient(Url? baseUrl = null)
{
    public const string DefaultBaseUrl = "https://coptic.askharoun.com/api";

    public Url BaseUrl { get; } = baseUrl ?? DefaultBaseUrl;

    public IFlurlRequest GetBase() => new FlurlRequest(BaseUrl.Clone());

    public async Task<AvailableContent> GetAvailableContentAsync()
        => await GetBase().AppendPathSegment("content").GetJsonAsync<AvailableContent>();

    public async Task<CustomContentResponse> PostCustomContentAsync(string type, Stream stream, string? sessionKey)
    {
        StreamContent content = new(stream);
        content.Headers.ContentType = new(ContentTypes.TypeToMime(type));

        var response = await GetBase()
            .AppendPathSegment("content")
            .SetQueryParam("sessionKey", sessionKey)
            .PostAsync(content);

        return await response.GetJsonAsync<CustomContentResponse>();
    }

    public async Task<Layout> GetLayoutAsync(string type, string id, string? sessionKey,
        DateTime? date = null, IEnumerable<LanguageInfo>? excludedLanguages = null)
    {
        var exclude = excludedLanguages?.Select(l => l.ToString());
        return await GetLayoutAsync(type, id, sessionKey, date, exclude);
    }

    public async Task<Layout> GetLayoutAsync(string type, string id, string? sessionKey,
        DateTime? date, IEnumerable<string>? excludedLanguageTags)
    {
        var request = GetBase()
            .AppendPathSegments("layout", type, id)
            .SetQueryParam("sessionKey", sessionKey);

        if (date is not null)
            request = request.SetQueryParam("date", date);

        if (excludedLanguageTags is not null)
            request = excludedLanguageTags
                .Aggregate(request, (current, tag) => current.AppendQueryParam("exclude", tag));

        var response = await request.GetStreamAsync();
        
        var xml = XDocument.Load(response);
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
}
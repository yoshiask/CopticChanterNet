using CoptLib.Models;
using CoptLib.Models.Sequences;

namespace CopticChanter.WebApi.ContentSources;

public interface IContentSource
{
    Task<Doc> GetDocAsync(string id);
    Task<DocSet> GetSetAsync(string id);
    Task<Sequence> GetSequenceAsync(string id);
}

public interface IModifiableContentSource : IContentSource
{
    Task AddAsync(Doc doc);
    Task AddAsync(DocSet set);
    Task AddAsync(Sequence seq);
}

public interface IEnumerableContentSource : IContentSource
{
    IAsyncEnumerable<Doc> GetDocsAsync();
    IAsyncEnumerable<DocSet> GetSetsAsync();
    IAsyncEnumerable<Sequence> GetSequencesAsync();
}

public static class ContentSourceExtensions
{
    public static async Task<Doc?> TryGetDocAsync(this IContentSource src, string id)
    {
        try
        {
            return await src.GetDocAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<DocSet?> TryGetSetAsync(this IContentSource src, string id)
    {
        try
        {
            return await src.GetSetAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<Sequence?> TryGetSequenceAsync(this IContentSource src, string id)
    {
        try
        {
            return await src.GetSequenceAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public static async Task<object?> TryGetAsync(this IContentSource src, string type, string id)
    {
        type = type.ToUpperInvariant();

        if (type == "DOC")
            return await src.TryGetDocAsync(id);
        else if (type == "SET")
            return await src.TryGetSetAsync(id);
        else if (type == "SEQ")
            return await src.TryGetSequenceAsync(id);

        return null;
    }
}

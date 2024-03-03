using CoptLib.Models;
using CoptLib.Models.Sequences;

namespace CopticChanter.WebApi.ContentSources;

public class MergedContentSource(IEnumerable<IContentSource>? sources) : IContentSource
{
    public List<IContentSource> Sources { get; } = new(sources ?? []);

    public Task<Doc> GetDocAsync(string id)
    {
        return GetAsync(id, (src, id) => src.TryGetDocAsync(id), "doc");
    }

    public Task<DocSet> GetSetAsync(string id)
    {
        return GetAsync(id, (src, id) => src.TryGetSetAsync(id), "set");
    }

    public Task<Sequence> GetSequenceAsync(string id)
    {
        return GetAsync(id, (src, id) => src.TryGetSequenceAsync(id), "sequence");
    }

    private async Task<T> GetAsync<T>(string id, Func<IContentSource, string, Task<T?>> get, string type)
    {
        foreach (var source in Sources)
        {
            var content = await get(source, id);
            if (content is not null)
                return content;
        }

        throw new Exception($"No {type} with ID {id} was found.");
    }
}

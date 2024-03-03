using CoptLib.Models;
using CoptLib.Models.Sequences;
using System.Collections.Concurrent;

namespace CopticChanter.WebApi.ContentSources;

public class MemoryContentSource : IModifiableContentSource, IEnumerableContentSource
{
    private readonly ConcurrentDictionary<string, Doc> _docs = [];
    private readonly ConcurrentDictionary<string, DocSet> _sets = [];
    private readonly ConcurrentDictionary<string, Sequence> _sequences = [];

    public Task AddAsync(Doc doc)
    {
        _docs.TryAdd(doc.Key ?? throw new Exception("Doc must specify a key."), doc);
        return Task.CompletedTask;
    }

    public Task AddAsync(DocSet set)
    {
        _sets.TryAdd(set.Key ?? throw new Exception("Set must specify a key."), set);
        return Task.CompletedTask;
    }

    public Task AddAsync(Sequence seq)
    {
        _sequences.TryAdd(seq.Key ?? throw new Exception("Sequence must specify a key."), seq);
        return Task.CompletedTask;
    }

    public async Task<Doc> GetDocAsync(string id) => _docs[id];

    public async Task<DocSet> GetSetAsync(string id) => _sets[id];

    public async Task<Sequence> GetSequenceAsync(string id) => _sequences[id];

    public IAsyncEnumerable<Doc> GetDocsAsync() => _docs.Values.ToAsyncEnumerable();

    public IAsyncEnumerable<DocSet> GetSetsAsync() => _sets.Values.ToAsyncEnumerable();

    public IAsyncEnumerable<Sequence> GetSequencesAsync() => _sequences.Values.ToAsyncEnumerable();
}

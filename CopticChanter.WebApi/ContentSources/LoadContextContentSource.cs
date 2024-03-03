using CoptLib.IO;
using CoptLib.Models;
using CoptLib.Models.Sequences;

namespace CopticChanter.WebApi.ContentSources;

public class LoadContextContentSource(ILoadContext _context) : IModifiableContentSource
{
    public async Task AddAsync(Doc doc) => _context.AddDoc(doc);

    public Task AddAsync(DocSet set) => throw new NotSupportedException();

    public Task AddAsync(Sequence seq) => throw new NotSupportedException();

    public async Task<Doc> GetDocAsync(string id) => _context.LoadedDocs.FirstOrDefault(d => d.Key == id)
        ?? throw new Exception($"No doc with id '{id}' was loaded");

    public Task<Sequence> GetSequenceAsync(string id) => throw new NotSupportedException();

    public Task<DocSet> GetSetAsync(string id) => throw new NotSupportedException();
}

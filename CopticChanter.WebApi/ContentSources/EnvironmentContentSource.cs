using CoptLib.IO;
using CoptLib.Models.Sequences;
using CoptLib.Models;
using System.Xml.Linq;

namespace CopticChanter.WebApi.ContentSources;

public class EnvironmentContentSource(IWebHostEnvironment _env, ILoadContext _context) : IContentSource
{
    public async Task<Doc> GetDocAsync(string id)
    {
        await using var stream = AvailableContent.Open("DOC", id, _env);
        return _context.LoadDoc(stream);
    }

    public async Task<DocSet> GetSetAsync(string id)
    {
        await using var stream = AvailableContent.Open("SET", id, _env);

        using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
        using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, id, id);
        DocSetReader setReader = new(setFolder, _context);
        await setReader.ReadDocs();

        return setReader.Set;
    }

    public async Task<Sequence> GetSequenceAsync(string id)
    {
        await using var stream = AvailableContent.Open("SEQ", id, _env);
        var seqXml = await XDocument.LoadAsync(stream, LoadOptions.None, default);
        return SequenceReader.ParseSequenceXml(seqXml, _context);
    }
}

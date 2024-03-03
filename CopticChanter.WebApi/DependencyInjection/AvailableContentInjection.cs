using CoptLib.IO;
using System.Xml.Linq;

namespace CopticChanter.WebApi.DependencyInjection;

public static class AvailableContentInjection
{
    public static IServiceCollection AddAvailableContent(this IServiceCollection services)
    {
        return services.AddSingleton(GetAvailableMetadata);
    }

    private static Core.AvailableContent GetAvailableMetadata(IServiceProvider services)
    {
        var env = services.GetRequiredService<IWebHostEnvironment>();

        Dictionary<string, string> docs = new(AvailableContent.Docs.Count);
        foreach (var id in AvailableContent.Docs.Keys)
        {
            var stream = AvailableContent.Open("DOC", id, env);

            var doc = DocReader.ReadDocXml(stream);
            docs.Add(id, doc.Name);
        }

        Dictionary<string, string> sets = new(AvailableContent.Sets.Count);
        foreach (var kvp in AvailableContent.Sets)
        {
            var id = kvp.Key;
            var file = kvp.Value;
            var stream = AvailableContent.Open("SET", id, env);

            using var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(stream);
            using var setFolder = new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, id, file);
            DocSetReader setReader = new(setFolder);
            setReader.ReadDocs().Wait();

            sets.Add(setReader.Set.Key!, setReader.Set.Name);
            
            foreach (var doc in setReader.Set.IncludedDocs)
            {
                if (docs.ContainsKey(doc.Key!))
                    continue;

                docs.Add(doc.Key!, doc.Name);
            }
        }

        Dictionary<string, string> sequences = new(AvailableContent.Sequences.Count);
        foreach (var id in AvailableContent.Sequences.Keys)
        {
            var stream = AvailableContent.Open("SEQ", id, env);

            var seqXml = XDocument.Load(stream, LoadOptions.None);
            var seq = SequenceReader.ParseSequenceXml(seqXml, new LoadContext());
            sequences.Add(seq.Key!, seq.Name!);
        }

        return new(docs, sets, sequences);
    }
}

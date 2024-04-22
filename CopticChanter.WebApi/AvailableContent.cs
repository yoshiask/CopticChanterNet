using OwlCore.Storage;

namespace CopticChanter.WebApi;

public static class AvailableContent
{   
    public static IReadOnlyDictionary<string, string> Docs = new Dictionary<string, string>
    {
        ["28c70071-ce5e-4add-aa5c-d093acfb2787"] = "../Sets/midnight-praises/docs/42.xml",
        ["urn:tasbehaorg-cr:1216"] = "Hymn of the Ten Strings.xml",
        ["urn:suscopts:seven-tunes"] = "Hymn of the Seven Tunes.xml",
        ["urn:tasbehaorg-cr:471"] = "../Sets/midnight-praises/docs/20 4 Thursday.xml",
        ["urn:tasbehaorg:91"] = "Psalm 151.xml",
    };

    public static IReadOnlyDictionary<string, string> Sequences = new Dictionary<string, string>
    {
        ["midnight-praises"] = "midnight-praises-sequence.xml",
        ["nativity-paramoun"] = "nativity-paramoun-sequence.xml",
    };

    public static IReadOnlyDictionary<string, string> Sets = new Dictionary<string, string>
    {
        ["urn:tasbeha:midnight-praises"] = "midnight-praises.zip",
        ["urn:suscopts:palm-sunday"] = "cop-palm-sunday.zip",
        ["urn:tasbeha:apostles-fast"] = "apostles-fast.zip",
        ["urn:mosc:nativity-feast"] = "malankara-nativity.zip",
        ["urn:tasbeha:nativity-paramone"] = "nativity-paramone.zip",
    };

    public static string GetDocPath(string id) => Path.Combine("content", "Docs", Docs[id]);
    public static string GetSetPath(string id) => Path.Combine("content", "Sets", Sets[id]);
    public static string GetSequencePath(string id) => Path.Combine("content", "Sequences", Sequences[id]);

    public static string GetPath(string type, string id)
    {
        return type.ToUpperInvariant() switch
        {
            "DOC" => GetDocPath(id),
            "SET" => GetSetPath(id),
            "SEQ" => GetSequencePath(id),
            _ => throw new ArgumentException(null, nameof(type))
        };
    }

    public static Stream Open(string type, string id, IWebHostEnvironment env)
    {
        return env.WebRootFileProvider.GetFileInfo(GetPath(type, id)).CreateReadStream();
    }
    
    public static IFolder OpenSetFolder(string id, IWebHostEnvironment env)
    {
        var setStream = Open("SET", id, env);
        var setArchive = SharpCompress.Archives.Zip.ZipArchive.Open(setStream);
        return new OwlCore.Storage.SharpCompress.ReadOnlyArchiveFolder(setArchive, id, id);
    }
}